namespace VirtualMachineLifecycle.Apps.Views;

public class UserDetailsBlade(int userId) : ViewBase
{
    public override object? Build()
    {
        var factory = this.UseService<VirtualMachineLifecycleContextFactory>();
        var blades = this.UseContext<IBladeController>();
        var refreshToken = this.UseRefreshToken();
        var user = this.UseState<User?>();
        var auditLogCount = this.UseState<int>();
        var snapshotCount = this.UseState<int>();
        var (alertView, showAlert) = this.UseAlert();

        this.UseEffect(async () =>
        {
            var db = factory.CreateDbContext();
            user.Set(await db.Users
                .Include(e => e.UserType)
                .SingleOrDefaultAsync(e => e.Id == userId));
            auditLogCount.Set(await db.AuditLogs.CountAsync(e => e.UserId == userId));
            snapshotCount.Set(await db.Snapshots.CountAsync(e => e.CreatedBy == userId));
        }, [EffectTrigger.AfterInit(), refreshToken]);

        if (user.Value == null) return null;

        var userValue = user.Value;

        void OnDelete()
        {
            showAlert("Are you sure you want to delete this user?", result =>
            {
                if (result.IsOk())
                {
                    Delete(factory);
                    blades.Pop(refresh: true);
                }
            }, "Delete User", AlertButtonSet.OkCancel);
        };

        var dropDown = Icons.Ellipsis
            .ToButton()
            .Ghost()
            .WithDropDown(
                MenuItem.Default("Delete").Icon(Icons.Trash).HandleSelect(OnDelete)
            );

        var editBtn = new Button("Edit")
            .Outline()
            .Icon(Icons.Pencil)
            .ToTrigger((isOpen) => new UserEditSheet(isOpen, refreshToken, userId));

        var detailsCard = new Card(
            content: new
                {
                    userValue.Id,
                    userValue.Username,
                    UserType = userValue.UserType.DescriptionText
                }.ToDetails()
                .RemoveEmpty()
                .Builder(e => e.Id, e => e.CopyToClipboard()),
            footer: Layout.Horizontal().Gap(2).Align(Align.Right)
                    | dropDown
                    | editBtn
        ).Title("User Details").Width(Size.Units(100));

        var relatedCard = new Card(
            new List(
                new ListItem("Audit Logs", onClick: _ =>
                {
                    blades.Push(this, new UserAuditLogsBlade(userId), "Audit Logs");
                }, badge: auditLogCount.Value.ToString("N0")),
                new ListItem("Snapshots", onClick: _ =>
                {
                    blades.Push(this, new UserSnapshotsBlade(userId), "Snapshots");
                }, badge: snapshotCount.Value.ToString("N0"))
            ));

        return new Fragment()
               | (Layout.Vertical() | detailsCard | relatedCard)
               | alertView;
    }

    private void Delete(VirtualMachineLifecycleContextFactory dbFactory)
    {
        using var db = dbFactory.CreateDbContext();
        var user = db.Users.FirstOrDefault(e => e.Id == userId)!;
        db.Users.Remove(user);
        db.SaveChanges();
    }
}