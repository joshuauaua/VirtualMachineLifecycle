using VirtualMachineLifecycle.Apps.Views;

namespace VirtualMachineLifecycle.Apps;

[App(icon: Icons.Server, path: ["Apps"])]
public class VMsApp : ViewBase
{
    public override object? Build()
    {
        return this.UseBlades(() => new VMListBlade(), "Search");
    }
}
