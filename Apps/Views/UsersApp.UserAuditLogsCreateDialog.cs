namespace VirtualMachineLifecycle.Apps.Views;

public class UserAuditLogsCreateDialog(IState<bool> isOpen, RefreshToken refreshToken, int userId) : ViewBase
{
    private record AuditLogCreateRequest
    {
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
            var auditLogId = CreateAuditLog(factory, auditLog.Value);
            refreshToken.Refresh(auditLogId);
        }, [auditLog]);

        return auditLog
            .ToForm()
            .Builder(e => e.ActionId, e => e.ToAsyncSelectInput(QueryActions(factory), LookupAction(factory), placeholder: "Select Action"))
            .ToDialog(isOpen, title: "Create Audit Log", submitTitle: "Create");
    }

    private int CreateAuditLog(VirtualMachineLifecycleContextFactory factory, AuditLogCreateRequest request)
    {
        using var db = factory.CreateDbContext();

        var auditLog = new AuditLog
        {
            UserId = userId,
            ActionId = request.ActionId,
            Details = request.Details,
            Timestamp = DateTime.UtcNow
        };

        db.AuditLogs.Add(auditLog);
        db.SaveChanges();

        return auditLog.Id;
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