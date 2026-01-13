using VirtualMachineLifecycle.Connections.VirtualMachineLifecycle;
using Microsoft.EntityFrameworkCore;
using VirtualMachineLifecycle.Apps.Views;

namespace VirtualMachineLifecycle.Apps;

[App(icon: Icons.ClipboardList, path: ["Apps"])]
public class AuditLogsApp : ViewBase
{
  public override object? Build()
  {
    try
    {
      var db = this.UseService<VirtualMachineLifecycleContext>();
      var client = this.UseService<IClientProvider>();
      var auditLogs = this.UseState<List<AuditLog>>([]);
      var selected = this.UseState<HashSet<int>>([]);
      var sortDescending = this.UseState(true);

      this.UseEffect(async () =>
      {
        var data = await db.AuditLogs
                      .Include(x => x.User)
                      .Include(x => x.Action)
                      .ToListAsync();
        auditLogs.Set(data);
      });

      var toolbar = new Card(
          Layout.Grid().Columns(2)
              | Text.Block("Audit Logs")
              | (Layout.Horizontal().Align(Align.Right)
                  | new Button("Sort Date", _ => sortDescending.Set(!sortDescending.Value))
                      .Icon(sortDescending.Value ? Icons.ChevronDown : Icons.ChevronUp)
                      .Variant(ButtonVariant.Outline)
                  | new Button("Actions")
                      .Icon(Icons.Menu)
                      .Variant(ButtonVariant.Outline)
                      .WithDropDown(
                          MenuItem.Default("Export Logs").HandleSelect(() => client.Toast("Export Logs triggered"))
                      )
                )
      );

      var sortedAuditLogs = sortDescending.Value
          ? auditLogs.Value.OrderByDescending(x => x.Timestamp)
          : auditLogs.Value.OrderBy(x => x.Timestamp);

      return new HeaderLayout(
          toolbar,
          new Card(
              Layout.Vertical()
                  | sortedAuditLogs.Select(x => new
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
                    UserName = x.User.Username,
                    Action = x.Action.DescriptionText,
                    Timestamp = x.Timestamp
                  }).ToTable().Width(Size.Full())
                  | Text.Block($"Total Logs: {auditLogs.Value.Count} | Selected: {selected.Value.Count}")
          ).Width(Size.Full())
      );
    }
    catch (Exception ex)
    {
      return Text.Block($"Error: {ex.Message}\nStack: {ex.StackTrace}");
    }
  }
}
