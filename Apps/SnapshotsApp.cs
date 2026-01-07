using VirtualMachineLifecycle.Apps.Views;

namespace VirtualMachineLifecycle.Apps;

[App(icon: Icons.Camera, path: ["Apps"])]
public class SnapshotsApp : ViewBase
{
    public override object? Build()
    {
        return this.UseBlades(() => new SnapshotListBlade(), "Search");
    }
}
