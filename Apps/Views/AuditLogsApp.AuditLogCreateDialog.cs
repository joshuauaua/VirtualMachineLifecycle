namespace VirtualMachineLifecycle.Apps.Views;

public class AuditLogCreateDialog(IState<bool> isOpen, RefreshToken refreshToken) : ViewBase
{
    private record AuditLogCreateRequest
    {
        [Required]
        public int UserId { get; init; }

        public int? VmId { get; init; }

        [Required]
        public int ActionId { get; init; }

        [Required]
        public DateTime Timestamp { get; init; } = DateTime.UtcNow;

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
            .Builder(e => e.UserId, e => e.ToAsyncSelectInput(QueryUsers(factory), LookupUser(factory), placeholder: "Select User"))
            .Builder(e => e.VmId, e => e.ToAsyncSelectInput(QueryVms(factory), LookupVm(factory), placeholder: "Select VM"))
            .Builder(e => e.ActionId, e => e.ToAsyncSelectInput(QueryActions(factory), LookupAction(factory), placeholder: "Select Action"))
            .Builder(e => e.Timestamp, e => e.ToDateTimeInput())
            .Builder(e => e.Details, e => e.ToTextAreaInput())
            .ToDialog(isOpen, title: "Create Audit Log", submitTitle: "Create");
    }

    private int CreateAuditLog(VirtualMachineLifecycleContextFactory factory, AuditLogCreateRequest request)
    {
        using var db = factory.CreateDbContext();

        var auditLog = new AuditLog
        {
            UserId = request.UserId,
            VmId = request.VmId,
            ActionId = request.ActionId,
            Timestamp = request.Timestamp,
            Details = request.Details
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