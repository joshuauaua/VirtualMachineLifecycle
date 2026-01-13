using VirtualMachineLifecycle.Connections.VirtualMachineLifecycle;
using Microsoft.EntityFrameworkCore;
using VirtualMachineLifecycle.Apps.Views;

namespace VirtualMachineLifecycle.Apps;

[App(icon: Icons.User, path: ["Apps"])]
public class UsersApp : ViewBase
{
  public override object? Build()
  {
    try
    {
      var db = this.UseService<VirtualMachineLifecycleContext>();
      var client = this.UseService<IClientProvider>();
      var users = this.UseState<List<User>>([]);
      var selected = this.UseState<HashSet<int>>([]);

      this.UseEffect(async () =>
      {
        var data = await db.Users
                      .Include(x => x.UserType)
                      .ToListAsync();
        users.Set(data);
      });

      var toolbar = new Card(
          Layout.Grid().Columns(2)
              | Text.Block("Users")
              | (Layout.Horizontal().Align(Align.Right)
                  | new Button("Actions")
                      .Icon(Icons.Menu)
                      .Variant(ButtonVariant.Outline)
                      .WithDropDown(
                          MenuItem.Default("Edit User").HandleSelect(() => client.Toast("Edit User triggered")),
                          MenuItem.Default("Delete User").HandleSelect(() => client.Toast("Delete User triggered"))
                      )
                )
      );

      return new HeaderLayout(
          toolbar,
          new Card(
              Layout.Vertical()
                  | users.Value.Select(x => new
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
                    x.Username,
                    UserType = x.UserType.DescriptionText
                  }).ToTable().Width(Size.Full())
                  | Text.Block($"Total Users: {users.Value.Count} | Selected: {selected.Value.Count}")
          ).Width(Size.Full())
      );
    }
    catch (Exception ex)
    {
      return Text.Block($"Error: {ex.Message}\nStack: {ex.StackTrace}");
    }
  }
}
