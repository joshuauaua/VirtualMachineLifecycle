namespace VirtualMachineLifecycle.Apps.Views;

public class UserSnapshotsBlade(int createdBy) : ViewBase
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
            snapshots.Set(await db.Snapshots
                .Include(s => s.Vm)
                .Where(s => s.CreatedBy == createdBy)
                .ToArrayAsync());
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
        }

        if (snapshots.Value == null) return null;

        var table = snapshots.Value.Select(s => new
            {
                Name = s.Name,
                VmName = s.Vm.Name,
                CreatedAt = s.CreatedAt,
                UpdatedAt = s.UpdatedAt,
                _ = Layout.Horizontal().Gap(2)
                    | Icons.Ellipsis
                        .ToButton()
                        .Ghost()
                        .WithDropDown(MenuItem.Default("Delete").Icon(Icons.Trash).HandleSelect(OnDelete(s.Id)))
                    | Icons.Pencil
                        .ToButton()
                        .Outline()
                        .Tooltip("Edit")
                        .ToTrigger((isOpen) => new UserSnapshotsEditSheet(isOpen, refreshToken, s.Id))
            })
            .ToTable()
            .RemoveEmptyColumns();

        var addBtn = new Button("Add Snapshot").Icon(Icons.Plus).Outline()
            .ToTrigger((isOpen) => new UserSnapshotsCreateDialog(isOpen, refreshToken, createdBy));

        return new Fragment()
               | BladeHelper.WithHeader(addBtn, table)
               | alertView;
    }

    public void Delete(VirtualMachineLifecycleContextFactory factory, int snapshotId)
    {
        using var db = factory.CreateDbContext();
        db.Snapshots.Remove(db.Snapshots.Single(s => s.Id == snapshotId));
        db.SaveChanges();
    }
}