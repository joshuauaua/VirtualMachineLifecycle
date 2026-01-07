/*
Tracks the number of virtual machines created each day.
SELECT CreatedAt AS Date, COUNT(Id) AS VMCount FROM Vms WHERE CreatedAt BETWEEN @StartDate AND @EndDate GROUP BY Date
*/
namespace VirtualMachineLifecycle.Apps.Views;

public class DailyVmCreationTrendLineChartView(DateTime startDate, DateTime endDate) : ViewBase
{
    public override object Build()
    {
        var factory = UseService<VirtualMachineLifecycleContextFactory>();
        var chart = UseState<object?>((object?)null);
        var exception = UseState<Exception?>((Exception?)null);

        this.UseEffect(async () =>
        {
            try
            {
                var db = factory.CreateDbContext();
                var data = await db.Vms
                    .Where(vm => vm.CreatedAt >= startDate && vm.CreatedAt <= endDate)
                    .GroupBy(vm => vm.CreatedAt.Date)
                    .Select(g => new
                    {
                        Date = g.Key.ToString("d MMM"),
                        VMCount = g.Count()
                    })
                    .ToListAsync();

                chart.Set(data.ToLineChart(
                    e => e.Date,
                    [e => e.Sum(f => (double)f.VMCount)],
                    LineChartStyles.Dashboard));
            }
            catch (Exception ex)
            {
                exception.Set(ex);
            }
        }, []);

        var card = new Card().Title("Daily VM Creation Trend").Height(Size.Units(80));

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