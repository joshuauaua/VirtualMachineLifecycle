namespace VirtualMachineLifecycle.Apps.Views;

public class VMSnapshotsBlade(int vmId) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<VirtualMachineLifecycleContextFactory>();
        var refreshToken = this.UseRefreshToken();
        var snapshots = this.UseState<Snapshot[]?>();
        var (alertView, showAlert) = this.UseAlert();

        this.UseEffect(async () =>
        {
            await using var db = factory.CreateDbContext();
            snapshots.Set(await db.Snapshots.Include(e => e.CreatedByNavigation).Where(e => e.VmId == vmId).ToArrayAsync());
        }, [ EffectTrigger.AfterInit(), refreshToken ]);

        Action OnDelete(int id)
        {
            return () =>
            {
                showAlert("Are you sure you want to delete this snapshot?", result =>
                {
                    if (result.IsOk())
                    {
                        Delete(factory, id);
                        refreshToken.Refresh();
                    }
                }, "Delete Snapshot", AlertButtonSet.OkCancel);
            };
        };

        if (snapshots.Value == null) return null;

        var table = snapshots.Value.Select(e => new
            {
                Name = e.Name,
                CreatedBy = e.CreatedByNavigation.Username,
                CreatedAt = e.CreatedAt,
                UpdatedAt = e.UpdatedAt,
                _ = Layout.Horizontal().Gap(2)
                    | Icons.Ellipsis
                        .ToButton()
                        .Ghost()
                        .WithDropDown(MenuItem.Default("Delete").Icon(Icons.Trash).HandleSelect(OnDelete(e.Id)))
                    | Icons.Pencil
                        .ToButton()
                        .Outline()
                        .Tooltip("Edit")
                        .ToTrigger((isOpen) => new VMSnapshotsEditSheet(isOpen, refreshToken, e.Id))
            })
            .ToTable()
            .RemoveEmptyColumns();

        var addBtn = new Button("Add Snapshot").Icon(Icons.Plus).Outline()
            .ToTrigger((isOpen) => new VMSnapshotsCreateDialog(isOpen, refreshToken, vmId));

        return new Fragment()
               | BladeHelper.WithHeader(addBtn, table)
               | alertView;
    }

    public void Delete(VirtualMachineLifecycleContextFactory factory, int snapshotId)
    {
        using var db2 = factory.CreateDbContext();
        db2.Snapshots.Remove(db2.Snapshots.Single(e => e.Id == snapshotId));
        db2.SaveChanges();
    }
}