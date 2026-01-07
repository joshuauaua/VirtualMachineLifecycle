/*
Displays the number of virtual machines managed by each provider.
SELECT Providers.DescriptionText AS Provider, COUNT(Vms.Id) AS VMCount FROM Vms JOIN Providers ON Vms.ProviderId = Providers.Id WHERE Vms.CreatedAt BETWEEN @StartDate AND @EndDate GROUP BY Providers.DescriptionText
*/
namespace VirtualMachineLifecycle.Apps.Views;

public class ProviderVmDistributionPieChartView(DateTime startDate, DateTime endDate) : ViewBase
{
    public override object Build()
    {
        var factory = UseService<VirtualMachineLifecycleContextFactory>();
        var chart = UseState<object?>((object?)null!);
        var exception = UseState<Exception?>((Exception?)null!);

        this.UseEffect(async () =>
        {
            try
            {
                var db = factory.CreateDbContext();
                var data = await db.Vms
                    .Where(vm => vm.CreatedAt >= startDate && vm.CreatedAt <= endDate)
                    .GroupBy(vm => vm.Provider.DescriptionText)
                    .Select(g => new
                    {
                        Provider = g.Key,
                        VMCount = g.Count()
                    })
                    .ToListAsync();

                var totalVMs = data.Sum(d => d.VMCount);

                PieChartTotal total = new(Format.Number(@"[<1000]0;[<10000]0.0,""K"";0,""K""", totalVMs), "VMs");

                chart.Set(data.ToPieChart(
                    dimension: d => d.Provider,
                    measure: d => d.Sum(x => (double)x.VMCount),
                    PieChartStyles.Dashboard,
                    total));
            }
            catch (Exception ex)
            {
                exception.Set(ex);
            }
        }, []);

        var card = new Card().Title("Provider VM Distribution").Height(Size.Units(80));

        if (exception.Value != null)
        {
            return card | new ErrorTeaserView(exception.Value);
        }

        if (chart.Value == null)
        {
            return card | new Skeleton();
        }

        return card | chart.Value;
    }
}