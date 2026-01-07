namespace VirtualMachineLifecycle.Apps.Views;

public class VMAuditLogsEditSheet(IState<bool> isOpen, RefreshToken refreshToken, int auditLogId) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<VirtualMachineLifecycleContextFactory>();
        var auditLog = UseState(() => factory.CreateDbContext().AuditLogs.FirstOrDefault(e => e.Id == auditLogId)!);

        UseEffect(() =>
        {
            using var db = factory.CreateDbContext();
            auditLog.Value.Timestamp = DateTime.UtcNow;
            db.AuditLogs.Update(auditLog.Value);
            db.SaveChanges();
            refreshToken.Refresh();
        }, [auditLog]);

        return auditLog
            .ToForm()
            .Builder(e => e.Details, e => e.ToTextAreaInput())
            .Builder(e => e.ActionId, e => e.ToAsyncSelectInput(QueryActions(factory), LookupAction(factory), placeholder: "Select Action"))
            .Builder(e => e.UserId, e => e.ToAsyncSelectInput(QueryUsers(factory), LookupUser(factory), placeholder: "Select User"))
            .Builder(e => e.VmId, e => e.ToAsyncSelectInput(QueryVMs(factory), LookupVM(factory), placeholder: "Select VM"))
            .Remove(e => e.Id, e => e.Timestamp)
            .ToSheet(isOpen, "Edit Audit Log");
    }

    private static AsyncSelectQueryDelegate<int?> QueryActions(VirtualMachineLifecycleContextFactory factory)
    {
        return async query =>
        {
            await using var db = factory.CreateDbContext();
            return (await db.AuditActions
                    .Where(e => e.DescriptionText.Contains(query))
                    .Select(e => new { e.Id, e.DescriptionText })
                    .Take(50)
                    .ToArrayAsync())
                .Select(e => new Option<int?>(e.DescriptionText, e.Id))
                .ToArray();
        };
    }

    private static AsyncSelectLookupDelegate<int?> LookupAction(VirtualMachineLifecycleContextFactory factory)
    {
        return async id =>
        {
            if (id == null) return null;
            await using var db = factory.CreateDbContext();
            var action = await db.AuditActions.FirstOrDefaultAsync(e => e.Id == id);
            if (action == null) return null;
            return new Option<int?>(action.DescriptionText, action.Id);
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

    private static AsyncSelectQueryDelegate<int?> QueryVMs(VirtualMachineLifecycleContextFactory factory)
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

    private static AsyncSelectLookupDelegate<int?> LookupVM(VirtualMachineLifecycleContextFactory factory)
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
}