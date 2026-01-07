using VirtualMachineLifecycle.Apps.Views;

namespace VirtualMachineLifecycle.Apps;

[App(icon: Icons.ClipboardList, path: ["Apps"])]
public class AuditLogsApp : ViewBase
{
    public override object? Build()
    {
        return this.UseBlades(() => new AuditLogListBlade(), "Search");
    }
}
