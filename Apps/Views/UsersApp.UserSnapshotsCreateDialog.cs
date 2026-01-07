namespace VirtualMachineLifecycle.Apps.Views;

public class UserSnapshotsCreateDialog(IState<bool> isOpen, RefreshToken refreshToken, int createdBy) : ViewBase
{
    private record SnapshotCreateRequest
    {
        [Required]
        public string Name { get; init; } = "";

        [Required]
        public int VmId { get; init; }
    }

    public override object? Build()
    {
        var factory = UseService<VirtualMachineLifecycleContextFactory>();
        var snapshot = UseState(() => new SnapshotCreateRequest());

        UseEffect(() =>
        {
            var snapshotId = CreateSnapshot(factory, snapshot.Value, createdBy);
            refreshToken.Refresh(snapshotId);
        }, [snapshot]);

        return snapshot
            .ToForm()
            .Builder(e => e.VmId, e => e.ToAsyncSelectInput(QueryVms(factory), LookupVm(factory), placeholder: "Select VM"))
            .ToDialog(isOpen, title: "Create Snapshot", submitTitle: "Create");
    }

    private int CreateSnapshot(VirtualMachineLifecycleContextFactory factory, SnapshotCreateRequest request, int createdBy)
    {
        using var db = factory.CreateDbContext();

        var snapshot = new Snapshot
        {
            Name = request.Name,
            VmId = request.VmId,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        db.Snapshots.Add(snapshot);
        db.SaveChanges();

        return snapshot.Id;
    }

    private static AsyncSelectQueryDelegate<int> QueryVms(VirtualMachineLifecycleContextFactory factory)
    {
        return async query =>
        {
            await using var db = factory.CreateDbContext();
            return (await db.Vms
                    .Where(e => e.Name.Contains(query))
                    .Select(e => new { e.Id, e.Name })
                    .Take(50)
                    .ToArrayAsync())
                .Select(e => new Option<int>(e.Name, e.Id))
                .ToArray();
        };
    }

    private static AsyncSelectLookupDelegate<int> LookupVm(VirtualMachineLifecycleContextFactory factory)
    {
        return async id =>
        {
            await using var db = factory.CreateDbContext();
            var vm = await db.Vms.FirstOrDefaultAsync(e => e.Id == id);
            if (vm == null) return null;
            return new Option<int>(vm.Name, vm.Id);
        };
    }
}