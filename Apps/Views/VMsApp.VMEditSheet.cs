namespace VirtualMachineLifecycle.Apps.Views;

public class VMEditSheet(IState<bool> isOpen, RefreshToken refreshToken, int vmId) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<VirtualMachineLifecycleContextFactory>();
        var vm = UseState(() => factory.CreateDbContext().Vms.Include(e => e.Provider).Include(e => e.VmStatus).FirstOrDefault(e => e.Id == vmId)!);

        UseEffect(() =>
        {
            using var db = factory.CreateDbContext();
            vm.Value.UpdatedAt = DateTime.UtcNow;
            db.Vms.Update(vm.Value);
            db.SaveChanges();
            refreshToken.Refresh();
        }, [vm]);

        return vm
            .ToForm()
            .Builder(e => e.Name, e => e.ToTextInput())
            .Builder(e => e.Tags, e => e.ToTextAreaInput())
            .Builder(e => e.LastAction, e => e.ToDateTimeInput())
            .Builder(e => e.ProviderId, e => e.ToAsyncSelectInput(QueryProviders(factory), LookupProvider(factory), placeholder: "Select Provider"))
            .Builder(e => e.VmStatusId, e => e.ToAsyncSelectInput(QueryVmStatuses(factory), LookupVmStatus(factory), placeholder: "Select VM Status"))
            .Remove(e => e.Id, e => e.CreatedAt, e => e.UpdatedAt, e => e.AuditLogs, e => e.Snapshots, e => e.Provider, e => e.VmStatus)
            .ToSheet(isOpen, "Edit VM");
    }

    private static AsyncSelectQueryDelegate<int> QueryProviders(VirtualMachineLifecycleContextFactory factory)
    {
        return async query =>
        {
            await using var db = factory.CreateDbContext();
            return (await db.Providers
                    .Where(e => e.DescriptionText.Contains(query))
                    .Select(e => new { e.Id, e.DescriptionText })
                    .Take(50)
                    .ToArrayAsync())
                .Select(e => new Option<int>(e.DescriptionText, e.Id))
                .ToArray();
        };
    }

    private static AsyncSelectLookupDelegate<int> LookupProvider(VirtualMachineLifecycleContextFactory factory)
    {
        return async id =>
        {
            await using var db = factory.CreateDbContext();
            var provider = await db.Providers.FirstOrDefaultAsync(e => e.Id == id);
            if (provider == null) return null;
            return new Option<int>(provider.DescriptionText, provider.Id);
        };
    }

    private static AsyncSelectQueryDelegate<int> QueryVmStatuses(VirtualMachineLifecycleContextFactory factory)
    {
        return async query =>
        {
            await using var db = factory.CreateDbContext();
            return (await db.VmStatuses
                    .Where(e => e.DescriptionText.Contains(query))
                    .Select(e => new { e.Id, e.DescriptionText })
                    .Take(50)
                    .ToArrayAsync())
                .Select(e => new Option<int>(e.DescriptionText, e.Id))
                .ToArray();
        };
    }

    private static AsyncSelectLookupDelegate<int> LookupVmStatus(VirtualMachineLifecycleContextFactory factory)
    {
        return async id =>
        {
            await using var db = factory.CreateDbContext();
            var vmStatus = await db.VmStatuses.FirstOrDefaultAsync(e => e.Id == id);
            if (vmStatus == null) return null;
            return new Option<int>(vmStatus.DescriptionText, vmStatus.Id);
        };
    }
}