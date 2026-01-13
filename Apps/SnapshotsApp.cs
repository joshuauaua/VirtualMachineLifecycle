using VirtualMachineLifecycle.Connections.VirtualMachineLifecycle;
using Microsoft.EntityFrameworkCore;
using VirtualMachineLifecycle.Apps.Views;

namespace VirtualMachineLifecycle.Apps;

[App(icon: Icons.Camera, path: ["Apps"])]
public class SnapshotsApp : ViewBase
{
  public override object? Build()
  {
    try
    {
      var db = this.UseService<VirtualMachineLifecycleContext>();
      var client = this.UseService<IClientProvider>();
      var snapshots = this.UseState<List<Snapshot>>([]);
      var selected = this.UseState<HashSet<int>>([]);
      var sortDescending = this.UseState(true);

      this.UseEffect(async () =>
      {
        var data = await db.Snapshots
                      .Include(x => x.Vm)
                      .Include(x => x.CreatedByNavigation)
                      .ToListAsync();
        snapshots.Set(data);
      });

      var toolbar = new Card(
          Layout.Grid().Columns(2)
              | Text.Block("Snapshots")
              | (Layout.Horizontal().Align(Align.Right)
                  | new Button("Sort Date", _ => sortDescending.Set(!sortDescending.Value))
                      .Icon(sortDescending.Value ? Icons.ChevronDown : Icons.ChevronUp)
                      .Variant(ButtonVariant.Outline)
                  | new Button("Actions")
                      .Icon(Icons.Menu)
                      .Variant(ButtonVariant.Outline)
                      .WithDropDown(
                          MenuItem.Default("Restore Snapshot").HandleSelect(() => client.Toast("Restore Snapshot triggered")),
                          MenuItem.Default("Delete Snapshot").HandleSelect(() => client.Toast("Delete Snapshot triggered"))
                      )
                )
      );

      var sortedSnapshots = sortDescending.Value
          ? snapshots.Value.OrderByDescending(x => x.CreatedAt)
          : snapshots.Value.OrderBy(x => x.CreatedAt);

      return new HeaderLayout(
          toolbar,
          new Card(
              Layout.Vertical()
                  | sortedSnapshots.Select(x => new
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
                    Name = x.Name,
                    VmName = x.Vm.Name,
                    CreatedBy = x.CreatedByNavigation.Username,
                    CreatedAt = x.CreatedAt
                  }).ToTable().Width(Size.Full())
                  | Text.Block($"Total Snapshots: {snapshots.Value.Count} | Selected: {selected.Value.Count}")
          ).Width(Size.Full())
      );
    }
    catch (Exception ex)
    {
      return Text.Block($"Error: {ex.Message}\nStack: {ex.StackTrace}");
    }
  }
}
