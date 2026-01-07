namespace VirtualMachineLifecycle.Apps.Views;

public class VMSnapshotsCreateDialog(IState<bool> isOpen, RefreshToken refreshToken, int vmId) : ViewBase
{
    private record SnapshotCreateRequest
    {
        [Required]
        public string Name { get; init; } = "";

        [Required]
        public int CreatedBy { get; init; }
    }

    public override object? Build()
    {
        var factory = UseService<VirtualMachineLifecycleContextFactory>();
        var snapshot = UseState(() => new SnapshotCreateRequest());

        UseEffect(() =>
        {
            var snapshotId = CreateSnapshot(factory, snapshot.Value, vmId);
            refreshToken.Refresh(snapshotId);
        }, [snapshot]);

        return snapshot
            .ToForm()
            .Builder(e => e.CreatedBy, e => e.ToAsyncSelectInput(QueryUsers(factory), LookupUser(factory), placeholder: "Select User"))
            .ToDialog(isOpen, title: "Create Snapshot", submitTitle: "Create");
    }

    private int CreateSnapshot(VirtualMachineLifecycleContextFactory factory, SnapshotCreateRequest request, int vmId)
    {
        using var db = factory.CreateDbContext();

        var snapshot = new Snapshot
        {
            Name = request.Name,
            CreatedBy = request.CreatedBy,
            VmId = vmId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        db.Snapshots.Add(snapshot);
        db.SaveChanges();

        return snapshot.Id;
    }

    private static AsyncSelectQueryDelegate<int> QueryUsers(VirtualMachineLifecycleContextFactory factory)
    {
        return async query =>
        {
            await using var db = factory.CreateDbContext();
            return (await db.Users
                    .Where(e => e.Username.Contains(query))
                    .Select(e => new { e.Id, e.Username })
                    .Take(50)
                    .ToArrayAsync())
                .Select(e => new Option<int>(e.Username, e.Id))
                .ToArray();
        };
    }

    private static AsyncSelectLookupDelegate<int> LookupUser(VirtualMachineLifecycleContextFactory factory)
    {
        return async id =>
        {
            await using var db = factory.CreateDbContext();
            var user = await db.Users.FirstOrDefaultAsync(e => e.Id == id);
            if (user == null) return null;
            return new Option<int>(user.Username, user.Id);
        };
    }
}