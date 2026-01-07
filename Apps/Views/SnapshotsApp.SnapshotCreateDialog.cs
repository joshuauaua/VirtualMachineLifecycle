namespace VirtualMachineLifecycle.Apps.Views;

public class SnapshotCreateDialog(IState<bool> isOpen, RefreshToken refreshToken) : ViewBase
{
    private record SnapshotCreateRequest
    {
        [Required]
        public int? VmId { get; init; } = null;

        [Required]
        public int? CreatedBy { get; init; } = null;

        [Required]
        public string Name { get; init; } = "";
    }

    public override object? Build()
    {
        var factory = UseService<VirtualMachineLifecycleContextFactory>();
        var snapshot = UseState(() => new SnapshotCreateRequest());

        UseEffect(() =>
        {
            var snapshotId = CreateSnapshot(factory, snapshot.Value);
            refreshToken.Refresh(snapshotId);
        }, [snapshot]);

        return snapshot
            .ToForm()
            .Builder(e => e.VmId, e => e.ToAsyncSelectInput(QueryVms(factory), LookupVm(factory), placeholder: "Select VM"))
            .Builder(e => e.CreatedBy, e => e.ToAsyncSelectInput(QueryUsers(factory), LookupUser(factory), placeholder: "Select User"))
            .ToDialog(isOpen, title: "Create Snapshot", submitTitle: "Create");
    }

    private int CreateSnapshot(VirtualMachineLifecycleContextFactory factory, SnapshotCreateRequest request)
    {
        using var db = factory.CreateDbContext();

        var snapshot = new Snapshot()
        {
            VmId = request.VmId!.Value,
            CreatedBy = request.CreatedBy!.Value,
            Name = request.Name,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        db.Snapshots.Add(snapshot);
        db.SaveChanges();

        return snapshot.Id;
    }

    private static AsyncSelectQueryDelegate<int?> QueryVms(VirtualMachineLifecycleContextFactory factory)
    {
        return async query =>
        {
            await using var db = factory.CreateDbContext();
            return (await db.Vms
                    .Where(e => e.Name.Contains(query))
                    .Select(e => new { e.Id, e.Name })
                    .Take(50)
                    .ToArrayAsync())
                .Select(e => new Option<int?>(e.Name, e.Id))
                .ToArray();
        };
    }

    private static AsyncSelectLookupDelegate<int?> LookupVm(VirtualMachineLifecycleContextFactory factory)
    {
        return async id =>
        {
            if (id == null) return null;
            await using var db = factory.CreateDbContext();
            var vm = await db.Vms.FirstOrDefaultAsync(e => e.Id == id);
            if (vm == null) return null;
            return new Option<int?>(vm.Name, vm.Id);
        };
    }

    private static AsyncSelectQueryDelegate<int?> QueryUsers(VirtualMachineLifecycleContextFactory factory)
    {
        return async query =>
        {
            await using var db = factory.CreateDbContext();
            return (await db.Users
                    .Where(e => e.Username.Contains(query))
                    .Select(e => new { e.Id, e.Username })
                    .Take(50)
                    .ToArrayAsync())
                .Select(e => new Option<int?>(e.Username, e.Id))
                .ToArray();
        };
    }

    private static AsyncSelectLookupDelegate<int?> LookupUser(VirtualMachineLifecycleContextFactory factory)
    {
        return async id =>
        {
            if (id == null) return null;
            await using var db = factory.CreateDbContext();
            var user = await db.Users.FirstOrDefaultAsync(e => e.Id == id);
            if (user == null) return null;
            return new Option<int?>(user.Username, user.Id);
        };
    }
}