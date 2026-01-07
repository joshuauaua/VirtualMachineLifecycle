/*
Tracks the number of virtual machines updated each day.
SELECT UpdatedAt AS Date, COUNT(Id) AS UpdateCount FROM Vms WHERE UpdatedAt BETWEEN @StartDate AND @EndDate GROUP BY Date
*/
namespace VirtualMachineLifecycle.Apps.Views;

public class DailyVmUpdatesLineChartView(DateTime startDate, DateTime endDate) : ViewBase
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
                    .Where(vm => vm.UpdatedAt >= startDate && vm.UpdatedAt <= endDate)
                    .GroupBy(vm => vm.UpdatedAt.Date)
                    .Select(g => new
                    {
                        Date = g.Key.ToString("d MMM"),
                        UpdateCount = g.Count()
                    })
                    .ToListAsync();

                chart.Set(data.ToLineChart(
                    e => e.Date,
                    [e => e.Sum(f => (double)f.UpdateCount)],
                    LineChartStyles.Dashboard));
            }
            catch (Exception ex)
            {
                exception.Set(ex);
            }
        }, []);

        var card = new Card().Title("Daily VM Updates").Height(Size.Units(80));

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