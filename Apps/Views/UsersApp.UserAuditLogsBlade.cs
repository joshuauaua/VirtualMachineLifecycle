namespace VirtualMachineLifecycle.Apps.Views;

public class UserAuditLogsBlade(int userId) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<VirtualMachineLifecycleContextFactory>();
        var refreshToken = this.UseRefreshToken();
        var auditLogs = this.UseState<AuditLog[]?>();
        var (alertView, showAlert) = this.UseAlert();

        this.UseEffect(async () =>
        {
            await using var db = factory.CreateDbContext();
            auditLogs.Set(await db.AuditLogs
                .Include(e => e.Action)
                .Include(e => e.Vm)
                .Where(e => e.UserId == userId)
                .ToArrayAsync());
        }, [ EffectTrigger.AfterInit(), refreshToken ]);

        Action OnDelete(int id)
        {
            return () =>
            {
                showAlert("Are you sure you want to delete this audit log?", result =>
                {
                    if (result.IsOk())
                    {
                        Delete(factory, id);
                        refreshToken.Refresh();
                    }
                }, "Delete Audit Log", AlertButtonSet.OkCancel);
            };
        }

        if (auditLogs.Value == null) return null;

        var table = auditLogs.Value.Select(e => new
            {
                Action = e.Action.DescriptionText,
                Vm = e.Vm?.Name ?? "N/A",
                Timestamp = e.Timestamp,
                Details = e.Details ?? "N/A",
                _ = Layout.Horizontal().Gap(2)
                    | Icons.Ellipsis
                        .ToButton()
                        .Ghost()
                        .WithDropDown(MenuItem.Default("Delete").Icon(Icons.Trash).HandleSelect(OnDelete(e.Id)))
                    | Icons.ChevronRight
                        .ToButton()
                        .Outline()
                        .Tooltip("Edit")
                        .ToTrigger((isOpen) => new UserAuditLogsEditSheet(isOpen, refreshToken, e.Id))
            })
            .ToTable()
            .RemoveEmptyColumns();

        var addBtn = new Button("Add Audit Log").Icon(Icons.Plus).Outline()
            .ToTrigger((isOpen) => new UserAuditLogsCreateDialog(isOpen, refreshToken, userId));

        return new Fragment()
               | BladeHelper.WithHeader(addBtn, table)
               | alertView;
    }

    public void Delete(VirtualMachineLifecycleContextFactory factory, int auditLogId)
    {
        using var db2 = factory.CreateDbContext();
        db2.AuditLogs.Remove(db2.AuditLogs.Single(e => e.Id == auditLogId));
        db2.SaveChanges();
    }
}