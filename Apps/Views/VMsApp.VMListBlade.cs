namespace VirtualMachineLifecycle.Apps.Views;

public class VMListBlade : ViewBase
{
    private record VMListRecord(int Id, string Name, string? Status);

    public override object? Build()
    {
        var blades = UseContext<IBladeController>();
        var factory = UseService<VirtualMachineLifecycleContextFactory>();
        var refreshToken = this.UseRefreshToken();

        UseEffect(() =>
        {
            if (refreshToken.ReturnValue is int vmId)
            {
                blades.Pop(this, true);
                blades.Push(this, new VMDetailsBlade(vmId));
            }
        }, [refreshToken]);

        var onItemClicked = new Action<Event<ListItem>>(e =>
        {
            var vm = (VMListRecord)e.Sender.Tag!;
            blades.Push(this, new VMDetailsBlade(vm.Id), vm.Name);
        });

        ListItem CreateItem(VMListRecord record) =>
            new(title: record.Name, subtitle: record.Status, onClick: onItemClicked, tag: record);

        var createBtn = Icons.Plus.ToButton(_ =>
        {
            blades.Pop(this);
        }).Ghost().Tooltip("Create VM").ToTrigger((isOpen) => new VMCreateDialog(isOpen, refreshToken));

        return new FilteredListView<VMListRecord>(
            fetchRecords: (filter) => FetchVMs(factory, filter),
            createItem: CreateItem,
            toolButtons: createBtn,
            onFilterChanged: _ =>
            {
                blades.Pop(this);
            }
        );
    }

    private async Task<VMListRecord[]> FetchVMs(VirtualMachineLifecycleContextFactory factory, string filter)
    {
        await using var db = factory.CreateDbContext();

        var linq = db.Vms.Include(vm => vm.VmStatus).AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter))
        {
            filter = filter.Trim();
            linq = linq.Where(vm => vm.Name.Contains(filter) || vm.VmStatus.DescriptionText.Contains(filter));
        }

        return await linq
            .OrderByDescending(vm => vm.CreatedAt)
            .Take(50)
            .Select(vm => new VMListRecord(vm.Id, vm.Name, vm.VmStatus.DescriptionText))
            .ToArrayAsync();
    }
}