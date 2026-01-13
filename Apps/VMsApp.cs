using VirtualMachineLifecycle.Connections.VirtualMachineLifecycle;
using Microsoft.EntityFrameworkCore;
using VirtualMachineLifecycle.Apps.Views;

namespace VirtualMachineLifecycle.Apps;

[App(icon: Icons.Server, path: ["Apps"])]
public class VMsApp : ViewBase
{
  public override object? Build()
  {
    try
    {
      var db = this.UseService<VirtualMachineLifecycleContext>();
      var client = this.UseService<IClientProvider>();
      var vms = this.UseState<List<Vm>>([]);
      var selected = this.UseState<HashSet<int>>([]);
      var sortDescending = this.UseState(true);

      this.UseEffect(async () =>
      {
        var data = await db.Vms
                .Include(x => x.Provider)
                .Include(x => x.VmStatus)
                .ToListAsync();
        vms.Set(data);
      });

      var toolbar = new Card(
          Layout.Grid().Columns(2)
              | Text.Block("Virtual Machines")
              | (Layout.Horizontal().Align(Align.Right)
                  | new Button("Sort Date", _ => sortDescending.Set(!sortDescending.Value))
                      .Icon(sortDescending.Value ? Icons.ChevronDown : Icons.ChevronUp)
                      .Variant(ButtonVariant.Outline)
                  | new Button("Actions")
                      .Icon(Icons.Menu)
                      .Variant(ButtonVariant.Outline)
                      .WithDropDown(
                          MenuItem.Default("Start VM").HandleSelect(() => client.Toast("Start VM triggered")),
                          MenuItem.Default("Stop VM").HandleSelect(() => client.Toast("Stop VM triggered"))
                      )
                )
      );

      var sortedVms = sortDescending.Value
          ? vms.Value.OrderByDescending(x => x.CreatedAt)
          : vms.Value.OrderBy(x => x.CreatedAt);

      return new HeaderLayout(
          toolbar,
          new Card(
              Layout.Vertical()
                  | sortedVms.Select(x => new
                  {
                    Select = selected.Value.Contains(x.Id)
                        ? Icons.Check.ToButton(_ =>
                        {
                          var newSet = new HashSet<int>(selected.Value);
                          newSet.Remove(x.Id);
                          selected.Set(newSet);
                        }).Ghost()
                        : Icons.Square.ToButton(_ =>
                        {
                          var newSet = new HashSet<int>(selected.Value);
                          newSet.Add(x.Id);
                          selected.Set(newSet);
                        }).Ghost(),
                    x.Name,
                    Status = x.VmStatus.DescriptionText,
                    Provider = x.Provider.DescriptionText,
                    LastAction = x.LastAction
                  }).ToTable().Width(Size.Full())
                  | Text.Block($"Total VMs: {vms.Value.Count} | Selected: {selected.Value.Count}")
          ).Width(Size.Full())
      );
    }
    catch (Exception ex)
    {
      return Text.Block($"Error: {ex.Message}\nStack: {ex.StackTrace}");
    }
  }
}
