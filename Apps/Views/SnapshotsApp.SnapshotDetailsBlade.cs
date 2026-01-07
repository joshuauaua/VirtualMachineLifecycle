namespace VirtualMachineLifecycle.Apps.Views;

public class SnapshotDetailsBlade(int snapshotId) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<VirtualMachineLifecycleContextFactory>();
        var blades = UseContext<IBladeController>();
        var refreshToken = this.UseRefreshToken();
        var snapshot = UseState<Snapshot?>(() => null!);
        var (alertView, showAlert) = this.UseAlert();

        UseEffect(async () =>
        {
            var db = factory.CreateDbContext();
            snapshot.Set(await db.Snapshots
                .Include(e => e.Vm)
                .Include(e => e.CreatedByNavigation)
                .SingleOrDefaultAsync(e => e.Id == snapshotId));
        }, [EffectTrigger.AfterInit(), refreshToken]);

        if (snapshot.Value == null) return null;

        var snapshotValue = snapshot.Value;

        var onDelete = () =>
        {
            showAlert("Are you sure you want to delete this snapshot?", result =>
            {
                if (result.IsOk())
                {
                    Delete(factory);
                    blades.Pop(refresh: true);
                }
            }, "Delete Snapshot", AlertButtonSet.OkCancel);
        };

        var dropDown = Icons.Ellipsis
            .ToButton()
            .Ghost()
            .WithDropDown(
                MenuItem.Default("Delete").Icon(Icons.Trash).HandleSelect(onDelete)
            );

        var editBtn = new Button("Edit")
            .Variant(ButtonVariant.Outline)
            .Icon(Icons.Pencil)
            .Width(Size.Grow())
            .ToTrigger((isOpen) => new SnapshotEditSheet(isOpen, refreshToken, snapshotId));

        var detailsCard = new Card(
            content: new
            {
                Id = snapshotValue.Id,
                Name = snapshotValue.Name,
                Vm = snapshotValue.Vm.Name,
                CreatedBy = snapshotValue.CreatedByNavigation.Username
            }
            .ToDetails()
            .RemoveEmpty()
            .Builder(e => e.Id, e => e.CopyToClipboard()),
            footer: Layout.Horizontal().Gap(2).Align(Align.Right)
                | dropDown
                | editBtn
        ).Title("Snapshot Details").Width(Size.Units(100));

        return new Fragment()
               | (Layout.Vertical() | detailsCard)
               | alertView;
    }

    private void Delete(VirtualMachineLifecycleContextFactory dbFactory)
    {
        using var db = dbFactory.CreateDbContext();
        var snapshot = db.Snapshots.FirstOrDefault(e => e.Id == snapshotId)!;
        db.Snapshots.Remove(snapshot);
        db.SaveChanges();
    }
}