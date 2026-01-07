namespace VirtualMachineLifecycle.Apps.Views;

public class VMDetailsBlade(int vmId) : ViewBase
{
    public override object? Build()
    {
        var factory = this.UseService<VirtualMachineLifecycleContextFactory>();
        var blades = this.UseContext<IBladeController>();
        var refreshToken = this.UseRefreshToken();
        var vm = this.UseState<Vm?>();
        var auditLogCount = this.UseState<int>();
        var snapshotCount = this.UseState<int>();
        var (alertView, showAlert) = this.UseAlert();

        this.UseEffect(async () =>
        {
            var db = factory.CreateDbContext();
            vm.Set(await db.Vms
                .Include(e => e.Provider)
                .Include(e => e.VmStatus)
                .SingleOrDefaultAsync(e => e.Id == vmId));
            auditLogCount.Set(await db.AuditLogs.CountAsync(e => e.VmId == vmId));
            snapshotCount.Set(await db.Snapshots.CountAsync(e => e.VmId == vmId));
        }, [EffectTrigger.AfterInit(), refreshToken]);

        if (vm.Value == null) return null;

        var vmValue = vm.Value;

        void OnDelete()
        {
            showAlert("Are you sure you want to delete this VM?", result =>
            {
                if (result.IsOk())
                {
                    Delete(factory);
                    blades.Pop(refresh: true);
                }
            }, "Delete VM", AlertButtonSet.OkCancel);
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
            .ToTrigger((isOpen) => new VMEditSheet(isOpen, refreshToken, vmId));

        var detailsCard = new Card(
            content: new
                {
                    vmValue.Id,
                    vmValue.Name,
                    Provider = vmValue.Provider.DescriptionText,
                    Status = vmValue.VmStatus.DescriptionText,
                    vmValue.Tags,
                    LastAction = vmValue.LastAction?.ToString("g") ?? "N/A"
                }.ToDetails()
                .RemoveEmpty()
                .Builder(e => e.Id, e => e.CopyToClipboard()),
            footer: Layout.Horizontal().Gap(2).Align(Align.Right)
                    | dropDown
                    | editBtn
        ).Title("VM Details").Width(Size.Units(100));

        var relatedCard = new Card(
            new List(
                new ListItem("Audit Logs", onClick: _ =>
                {
                    blades.Push(this, new VMAuditLogsBlade(vmId), "Audit Logs");
                }, badge: auditLogCount.Value.ToString("N0")),
                new ListItem("Snapshots", onClick: _ =>
                {
                    blades.Push(this, new VMSnapshotsBlade(vmId), "Snapshots");
                }, badge: snapshotCount.Value.ToString("N0"))
            ));

        return new Fragment()
               | (Layout.Vertical() | detailsCard | relatedCard)
               | alertView;
    }

    private void Delete(VirtualMachineLifecycleContextFactory dbFactory)
    {
        using var db = dbFactory.CreateDbContext();
        var vm = db.Vms.FirstOrDefault(e => e.Id == vmId)!;
        db.Vms.Remove(vm);
        db.SaveChanges();
    }
}