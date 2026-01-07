namespace VirtualMachineLifecycle.Apps.Views;

public class UserEditSheet(IState<bool> isOpen, RefreshToken refreshToken, int userId) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<VirtualMachineLifecycleContextFactory>();
        var user = UseState(() => factory.CreateDbContext().Users.FirstOrDefault(e => e.Id == userId)!);

        UseEffect(() =>
        {
            using var db = factory.CreateDbContext();
            user.Value.UpdatedAt = DateTime.UtcNow;
            db.Users.Update(user.Value);
            db.SaveChanges();
            refreshToken.Refresh();
        }, [user]);

        return user
            .ToForm()
            .Remove(e => e.Id, e => e.CreatedAt, e => e.UpdatedAt)
            .Builder(e => e.UserTypeId, e => e.ToAsyncSelectInput(QueryUserTypes(factory), LookupUserType(factory), placeholder: "Select User Type"))
            .ToSheet(isOpen, "Edit User");
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