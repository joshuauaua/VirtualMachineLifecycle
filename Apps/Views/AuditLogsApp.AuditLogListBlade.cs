namespace VirtualMachineLifecycle.Apps.Views;

public class AuditLogListBlade : ViewBase
{
    private record AuditLogListRecord(int Id, string UserName, string ActionDescription, DateTime Timestamp);

    public override object? Build()
    {
        var blades = UseContext<IBladeController>();
        var factory = UseService<VirtualMachineLifecycleContextFactory>();
        var refreshToken = this.UseRefreshToken();

        UseEffect(() =>
        {
            if (refreshToken.ReturnValue is int auditLogId)
            {
                blades.Pop(this, true);
                blades.Push(this, new AuditLogDetailsBlade(auditLogId));
            }
        }, [refreshToken]);

        var onItemClicked = new Action<Event<ListItem>>(e =>
        {
            var auditLog = (AuditLogListRecord)e.Sender.Tag!;
            blades.Push(this, new AuditLogDetailsBlade(auditLog.Id), auditLog.UserName);
        });

        ListItem CreateItem(AuditLogListRecord record) =>
            new(title: record.UserName, subtitle: $"{record.ActionDescription} - {record.Timestamp:O}", onClick: onItemClicked, tag: record);

        var createBtn = Icons.Plus.ToButton(_ =>
        {
            blades.Pop(this);
        }).Ghost().Tooltip("Create Audit Log").ToTrigger((isOpen) => new AuditLogCreateDialog(isOpen, refreshToken));

        return new FilteredListView<AuditLogListRecord>(
            fetchRecords: (filter) => FetchAuditLogs(factory, filter),
            createItem: CreateItem,
            toolButtons: createBtn,
            onFilterChanged: _ =>
            {
                blades.Pop(this);
            }
        );
    }

    private async Task<AuditLogListRecord[]> FetchAuditLogs(VirtualMachineLifecycleContextFactory factory, string filter)
    {
        await using var db = factory.CreateDbContext();

        var linq = db.AuditLogs
            .Include(a => a.User)
            .Include(a => a.Action)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter))
        {
            filter = filter.Trim();
            linq = linq.Where(a => a.User.Username.Contains(filter) || a.Action.DescriptionText.Contains(filter));
        }

        return await linq
            .OrderByDescending(a => a.Timestamp)
            .Take(50)
            .Select(a => new AuditLogListRecord(a.Id, a.User.Username, a.Action.DescriptionText, a.Timestamp))
            .ToArrayAsync();
    }
}