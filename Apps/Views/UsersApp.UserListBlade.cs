namespace VirtualMachineLifecycle.Apps.Views;

public class UserListBlade : ViewBase
{
    private record UserListRecord(int Id, string Username, string UserType);

    public override object? Build()
    {
        var blades = UseContext<IBladeController>();
        var factory = UseService<VirtualMachineLifecycleContextFactory>();
        var refreshToken = this.UseRefreshToken();

        UseEffect(() =>
        {
            if (refreshToken.ReturnValue is int userId)
            {
                blades.Pop(this, true);
                blades.Push(this, new UserDetailsBlade(userId));
            }
        }, [refreshToken]);

        var onItemClicked = new Action<Event<ListItem>>(e =>
        {
            var user = (UserListRecord)e.Sender.Tag!;
            blades.Push(this, new UserDetailsBlade(user.Id), user.Username);
        });

        ListItem CreateItem(UserListRecord record) =>
            new(title: record.Username, subtitle: record.UserType, onClick: onItemClicked, tag: record);

        var createBtn = Icons.Plus.ToButton(_ =>
        {
            blades.Pop(this);
        }).Ghost().Tooltip("Create User").ToTrigger((isOpen) => new UserCreateDialog(isOpen, refreshToken));

        return new FilteredListView<UserListRecord>(
            fetchRecords: (filter) => FetchUsers(factory, filter),
            createItem: CreateItem,
            toolButtons: createBtn,
            onFilterChanged: _ =>
            {
                blades.Pop(this);
            }
        );
    }

    private async Task<UserListRecord[]> FetchUsers(VirtualMachineLifecycleContextFactory factory, string filter)
    {
        await using var db = factory.CreateDbContext();

        var linq = db.Users.Include(u => u.UserType).AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter))
        {
            filter = filter.Trim();
            linq = linq.Where(u => u.Username.Contains(filter) || u.UserType.DescriptionText.Contains(filter));
        }

        return await linq
            .OrderByDescending(u => u.CreatedAt)
            .Take(50)
            .Select(u => new UserListRecord(u.Id, u.Username, u.UserType.DescriptionText))
            .ToArrayAsync();
    }
}