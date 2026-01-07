/*
Displays the distribution of virtual machines across different statuses.
SELECT VmStatus.DescriptionText AS Status, COUNT(Vms.Id) AS VMCount FROM Vms JOIN VmStatus ON Vms.VmStatusId = VmStatus.Id WHERE Vms.CreatedAt BETWEEN @StartDate AND @EndDate GROUP BY VmStatus.DescriptionText
*/
namespace VirtualMachineLifecycle.Apps.Views;

public class VmStatusDistributionPieChartView(DateTime startDate, DateTime endDate) : ViewBase
{
    public override object? Build()
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
                    .GroupBy(vm => vm.VmStatus.DescriptionText)
                    .Select(g => new
                    {
                        Status = g.Key,
                        VMCount = g.Count()
                    })
                    .ToListAsync();

                var totalVMs = data.Sum(d => d.VMCount);

                PieChartTotal total = new(Format.Number(@"[<1000]0;[<10000]0.0,""K"";0,""K""", totalVMs), "VMs");

                chart.Set(data.ToPieChart(
                    dimension: d => d.Status,
                    measure: d => d.Sum(x => (double)x.VMCount),
                    PieChartStyles.Dashboard,
                    total));
            }
            catch (Exception ex)
            {
                exception.Set(ex);
            }
        }, []);

        var card = new Card().Title("VM Status Distribution").Height(Size.Units(80));

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