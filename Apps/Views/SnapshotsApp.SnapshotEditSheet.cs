namespace VirtualMachineLifecycle.Apps.Views;

public class SnapshotEditSheet(IState<bool> isOpen, RefreshToken refreshToken, int snapshotId) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<VirtualMachineLifecycleContextFactory>();
        var snapshot = UseState(() => factory.CreateDbContext().Snapshots.FirstOrDefault(e => e.Id == snapshotId)!);

        UseEffect(() =>
        {
            using var db = factory.CreateDbContext();
            snapshot.Value.UpdatedAt = DateTime.UtcNow;
            db.Snapshots.Update(snapshot.Value);
            db.SaveChanges();
            refreshToken.Refresh();
        }, [snapshot]);

        return snapshot
            .ToForm()
            .Builder(e => e.Name, e => e.ToTextInput())
            .Builder(e => e.VmId, e => e.ToAsyncSelectInput(QueryVms(factory), LookupVm(factory), placeholder: "Select VM"))
            .Builder(e => e.CreatedBy, e => e.ToAsyncSelectInput(QueryUsers(factory), LookupUser(factory), placeholder: "Select User"))
            .Remove(e => e.Id, e => e.CreatedAt, e => e.UpdatedAt)
            .ToSheet(isOpen, "Edit Snapshot");
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