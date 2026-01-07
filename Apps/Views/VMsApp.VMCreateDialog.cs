namespace VirtualMachineLifecycle.Apps.Views;

public class VMCreateDialog(IState<bool> isOpen, RefreshToken refreshToken) : ViewBase
{
    private record VMCreateRequest
    {
        [Required]
        public string Name { get; init; } = "";

        [Required]
        public int? VmStatusId { get; init; } = null;

        [Required]
        public int? ProviderId { get; init; } = null;

        public string? Tags { get; init; } = null;
    }

    public override object? Build()
    {
        var factory = UseService<VirtualMachineLifecycleContextFactory>();
        var vm = UseState(() => new VMCreateRequest());

        UseEffect(() =>
        {
            var vmId = CreateVM(factory, vm.Value);
            refreshToken.Refresh(vmId);
        }, [vm]);

        return vm
            .ToForm()
            .Builder(e => e.VmStatusId, e => e.ToAsyncSelectInput(QueryVmStatuses(factory), LookupVmStatus(factory), placeholder: "Select VM Status"))
            .Builder(e => e.ProviderId, e => e.ToAsyncSelectInput(QueryProviders(factory), LookupProvider(factory), placeholder: "Select Provider"))
            .ToDialog(isOpen, title: "Create VM", submitTitle: "Create");
    }

    private int CreateVM(VirtualMachineLifecycleContextFactory factory, VMCreateRequest request)
    {
        using var db = factory.CreateDbContext();

        var vm = new Vm()
        {
            Name = request.Name,
            VmStatusId = request.VmStatusId!.Value,
            ProviderId = request.ProviderId!.Value,
            Tags = request.Tags,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        db.Vms.Add(vm);
        db.SaveChanges();

        return vm.Id;
    }

    private static AsyncSelectQueryDelegate<int?> QueryVmStatuses(VirtualMachineLifecycleContextFactory factory)
    {
        return async query =>
        {
            await using var db = factory.CreateDbContext();
            return (await db.VmStatuses
                    .Where(e => e.DescriptionText.Contains(query))
                    .Select(e => new { e.Id, e.DescriptionText })
                    .Take(50)
                    .ToArrayAsync())
                .Select(e => new Option<int?>(e.DescriptionText, e.Id))
                .ToArray();
        };
    }

    private static AsyncSelectLookupDelegate<int?> LookupVmStatus(VirtualMachineLifecycleContextFactory factory)
    {
        return async id =>
        {
            if (id == null) return null;
            await using var db = factory.CreateDbContext();
            var status = await db.VmStatuses.FirstOrDefaultAsync(e => e.Id == id);
            if (status == null) return null;
            return new Option<int?>(status.DescriptionText, status.Id);
        };
    }

    private static AsyncSelectQueryDelegate<int?> QueryProviders(VirtualMachineLifecycleContextFactory factory)
    {
        return async query =>
        {
            await using var db = factory.CreateDbContext();
            return (await db.Providers
                    .Where(e => e.DescriptionText.Contains(query))
                    .Select(e => new { e.Id, e.DescriptionText })
                    .Take(50)
                    .ToArrayAsync())
                .Select(e => new Option<int?>(e.DescriptionText, e.Id))
                .ToArray();
        };
    }

    private static AsyncSelectLookupDelegate<int?> LookupProvider(VirtualMachineLifecycleContextFactory factory)
    {
        return async id =>
        {
            if (id == null) return null;
            await using var db = factory.CreateDbContext();
            var provider = await db.Providers.FirstOrDefaultAsync(e => e.Id == id);
            if (provider == null) return null;
            return new Option<int?>(provider.DescriptionText, provider.Id);
        };
    }
}