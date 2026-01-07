namespace VirtualMachineLifecycle.Apps.Views;

public class AuditLogDetailsBlade(int auditLogId) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<VirtualMachineLifecycleContextFactory>();
        var blades = UseContext<IBladeController>();
        var refreshToken = this.UseRefreshToken();
        var auditLog = UseState<AuditLog?>(() => null!);
        var (alertView, showAlert) = this.UseAlert();

        UseEffect(async () =>
        {
            var db = factory.CreateDbContext();
            auditLog.Set(await db.AuditLogs
                .Include(e => e.Action)
                .Include(e => e.User)
                .Include(e => e.Vm)
                .SingleOrDefaultAsync(e => e.Id == auditLogId));
        }, [EffectTrigger.AfterInit(), refreshToken]);

        if (auditLog.Value == null) return null;

        var auditLogValue = auditLog.Value;

        var onDelete = () =>
        {
            showAlert("Are you sure you want to delete this audit log?", result =>
            {
                if (result.IsOk())
                {
                    Delete(factory);
                    blades.Pop(refresh: true);
                }
            }, "Delete Audit Log", AlertButtonSet.OkCancel);
        };

        var dropDown = Icons.Ellipsis
            .ToButton()
            .Ghost()
            .WithDropDown(
                MenuItem.Default("Delete").Icon(Icons.Trash).HandleSelect(onDelete)
            );

        var editBtn = new Button("Edit")
            .Variant(ButtonVariant.Outline)
            .Icon(Icons.Pencil)
            .Width(Size.Grow())
            .ToTrigger((isOpen) => new AuditLogEditSheet(isOpen, refreshToken, auditLogId));

        var detailsCard = new Card(
            content: new
                {
                    Id = auditLogValue.Id,
                    User = auditLogValue.User.Username,
                    Action = auditLogValue.Action.DescriptionText,
                    Vm = auditLogValue.Vm?.Name ?? "N/A",
                    Timestamp = auditLogValue.Timestamp,
                    Details = auditLogValue.Details
                }
                .ToDetails()
                .MultiLine(e => e.Details)
                .RemoveEmpty()
                .Builder(e => e.Id, e => e.CopyToClipboard()),
            footer: Layout.Horizontal().Gap(2).Align(Align.Right)
                    | dropDown
                    | editBtn
        ).Title("Audit Log Details").Width(Size.Units(100));

        return new Fragment()
               | (Layout.Vertical() | detailsCard)
               | alertView;
    }

    private void Delete(VirtualMachineLifecycleContextFactory dbFactory)
    {
        using var db = dbFactory.CreateDbContext();
        var auditLog = db.AuditLogs.FirstOrDefault(e => e.Id == auditLogId)!;
        db.AuditLogs.Remove(auditLog);
        db.SaveChanges();
    }
}