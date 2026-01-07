namespace VirtualMachineLifecycle.Apps.Views;

public class UserCreateDialog(IState<bool> isOpen, RefreshToken refreshToken) : ViewBase
{
    private record UserCreateRequest
    {
        [Required]
        public string Username { get; init; } = "";

        [Required]
        public int? UserTypeId { get; init; } = null;
    }

    public override object? Build()
    {
        var factory = UseService<VirtualMachineLifecycleContextFactory>();
        var user = UseState(() => new UserCreateRequest());

        UseEffect(() =>
        {
            var userId = CreateUser(factory, user.Value);
            refreshToken.Refresh(userId);
        }, [user]);

        return user
            .ToForm()
            .Builder(e => e.UserTypeId, e => e.ToAsyncSelectInput(QueryUserTypes(factory), LookupUserType(factory), placeholder: "Select User Type"))
            .ToDialog(isOpen, title: "Create User", submitTitle: "Create");
    }

    private int CreateUser(VirtualMachineLifecycleContextFactory factory, UserCreateRequest request)
    {
        using var db = factory.CreateDbContext();

        var user = new User()
        {
            Username = request.Username,
            UserTypeId = request.UserTypeId!.Value,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        db.Users.Add(user);
        db.SaveChanges();

        return user.Id;
    }

    private static AsyncSelectQueryDelegate<int?> QueryUserTypes(VirtualMachineLifecycleContextFactory factory)
    {
        return async query =>
        {
            await using var db = factory.CreateDbContext();
            return (await db.UserTypes
                    .Where(e => e.DescriptionText.Contains(query))
                    .Select(e => new { e.Id, e.DescriptionText })
                    .Take(50)
                    .ToArrayAsync())
                .Select(e => new Option<int?>(e.DescriptionText, e.Id))
                .ToArray();
        };
    }

    private static AsyncSelectLookupDelegate<int?> LookupUserType(VirtualMachineLifecycleContextFactory factory)
    {
        return async id =>
        {
            if (id == null) return null;
            await using var db = factory.CreateDbContext();
            var userType = await db.UserTypes.FirstOrDefaultAsync(e => e.Id == id);
            if (userType == null) return null;
            return new Option<int?>(userType.DescriptionText, userType.Id);
        };
    }
}