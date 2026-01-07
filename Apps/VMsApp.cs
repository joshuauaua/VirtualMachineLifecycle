using VirtualMachineLifecycle.Connections.VirtualMachineLifecycle;
using Microsoft.EntityFrameworkCore;
using VirtualMachineLifecycle.Apps.Views;

namespace VirtualMachineLifecycle.Apps;

[App(icon: Icons.Server, path: ["Apps"])]
public class VMsApp : ViewBase
{
  public override object? Build()
  {
    var db = this.UseService<VirtualMachineLifecycleContext>();
    var vms = this.UseState<List<Vm>>([]);

    this.UseEffect(async () =>
    {
      var data = await db.Vms
              .Include(x => x.Provider)
              .Include(x => x.VmStatus)
              .ToListAsync();
      vms.Set(data);
    });

    return Layout.Vertical()
        | new Card(
            Layout.Vertical()
                | vms.Value.Select(x => new
                {
                  x.Name,
                  Status = x.VmStatus.DescriptionText,
                  Provider = x.Provider.DescriptionText,
                  LastAction = x.LastAction
                }).ToTable().Width(Size.Full())
                | Text.Block($"Total VMs: {vms.Value.Count}")
        ).Title("Virtual Machines").Width(Size.Full());
  }
}
