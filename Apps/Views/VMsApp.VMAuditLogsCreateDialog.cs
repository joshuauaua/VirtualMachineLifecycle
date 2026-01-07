namespace VirtualMachineLifecycle.Apps.Views;

public class VMAuditLogsCreateDialog(IState<bool> isOpen, RefreshToken refreshToken, int? vmId) : ViewBase
{
    private record AuditLogCreateRequest
    {
        [Required]
        public int UserId { get; init; }

        [Required]
        public int ActionId { get; init; }

        public string? Details { get; init; }
    }

    public override object? Build()
    {
        var factory = UseService<VirtualMachineLifecycleContextFactory>();
        var auditLog = UseState(() => new AuditLogCreateRequest());

        UseEffect(() =>
        {
            var auditLogId = CreateAuditLog(factory, auditLog.Value, vmId);
            refreshToken.Refresh(auditLogId);
        }, [auditLog]);

        return auditLog
            .ToForm()
            .Builder(e => e.UserId, e => e.ToAsyncSelectInput(QueryUsers(factory), LookupUser(factory), placeholder: "Select User"))
            .Builder(e => e.ActionId, e => e.ToAsyncSelectInput(QueryActions(factory), LookupAction(factory), placeholder: "Select Action"))
            .Builder(e => e.Details, e => e.ToTextAreaInput())
            .ToDialog(isOpen, title: "Create Audit Log", submitTitle: "Create");
    }

    private int CreateAuditLog(VirtualMachineLifecycleContextFactory factory, AuditLogCreateRequest request, int? vmId)
    {
        using var db = factory.CreateDbContext();

        var auditLog = new AuditLog
        {
            UserId = request.UserId,
            ActionId = request.ActionId,
            Details = request.Details,
            VmId = vmId,
            Timestamp = DateTime.UtcNow
        };

        db.AuditLogs.Add(auditLog);
        db.SaveChanges();

        return auditLog.Id;
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

    private static AsyncSelectQueryDelegate<int> QueryActions(VirtualMachineLifecycleContextFactory factory)
    {
        return async query =>
        {
            await using var db = factory.CreateDbContext();
            return (await db.AuditActions
                    .Where(e => e.DescriptionText.Contains(query))
                    .Select(e => new { e.Id, e.DescriptionText })
                    .Take(50)
                    .ToArrayAsync())
                .Select(e => new Option<int>(e.DescriptionText, e.Id))
                .ToArray();
        };
    }

    private static AsyncSelectLookupDelegate<int> LookupAction(VirtualMachineLifecycleContextFactory factory)
    {
        return async id =>
        {
            await using var db = factory.CreateDbContext();
            var action = await db.AuditActions.FirstOrDefaultAsync(e => e.Id == id);
            if (action == null) return null;
            return new Option<int>(action.DescriptionText, action.Id);
        };
    }
}