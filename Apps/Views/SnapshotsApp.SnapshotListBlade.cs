namespace VirtualMachineLifecycle.Apps.Views;

public class SnapshotListBlade : ViewBase
{
    private record SnapshotListRecord(int Id, string Name, string VmName, string CreatedBy, DateTime CreatedAt);

    public override object? Build()
    {
        var blades = UseContext<IBladeController>();
        var factory = UseService<VirtualMachineLifecycleContextFactory>();
        var refreshToken = this.UseRefreshToken();

        UseEffect(() =>
        {
            if (refreshToken.ReturnValue is int snapshotId)
            {
                blades.Pop(this, true);
                blades.Push(this, new SnapshotDetailsBlade(snapshotId));
            }
        }, [refreshToken]);

        var onItemClicked = new Action<Event<ListItem>>(e =>
        {
            var snapshot = (SnapshotListRecord)e.Sender.Tag!;
            blades.Push(this, new SnapshotDetailsBlade(snapshot.Id), snapshot.Name);
        });

        ListItem CreateItem(SnapshotListRecord record) =>
            new(title: record.Name, subtitle: $"{record.VmName} | {record.CreatedBy} | {record.CreatedAt:yyyy-MM-dd}", onClick: onItemClicked, tag: record);

        var createBtn = Icons.Plus.ToButton(_ =>
        {
            blades.Pop(this);
        }).Ghost().Tooltip("Create Snapshot").ToTrigger((isOpen) => new SnapshotCreateDialog(isOpen, refreshToken));

        return new FilteredListView<SnapshotListRecord>(
            fetchRecords: (filter) => FetchSnapshots(factory, filter),
            createItem: CreateItem,
            toolButtons: createBtn,
            onFilterChanged: _ =>
            {
                blades.Pop(this);
            }
        );
    }

    private async Task<SnapshotListRecord[]> FetchSnapshots(VirtualMachineLifecycleContextFactory factory, string filter)
    {
        await using var db = factory.CreateDbContext();

        var linq = db.Snapshots
            .Include(s => s.Vm)
            .Include(s => s.CreatedByNavigation)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter))
        {
            filter = filter.Trim();
            linq = linq.Where(s => s.Name.Contains(filter) || s.Vm.Name.Contains(filter) || s.CreatedByNavigation.Username.Contains(filter));
        }

        return await linq
            .OrderByDescending(s => s.CreatedAt)
            .Take(50)
            .Select(s => new SnapshotListRecord(
                s.Id,
                s.Name,
                s.Vm.Name,
                s.CreatedByNavigation.Username,
                s.CreatedAt
            ))
            .ToArrayAsync();
    }
}