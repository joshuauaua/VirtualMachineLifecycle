/*
Tracks the number of snapshots created each day.
SELECT CreatedAt AS Date, COUNT(Id) AS SnapshotCount FROM Snapshots WHERE CreatedAt BETWEEN @StartDate AND @EndDate GROUP BY Date
*/
namespace VirtualMachineLifecycle.Apps.Views;

public class DailySnapshotCreationTrendLineChartView(DateTime startDate, DateTime endDate) : ViewBase
{
    public override object Build()
    {
        var factory = this.UseService<VirtualMachineLifecycleContextFactory>();
        var chart = this.UseState<object?>((object?)null);
        var exception = this.UseState<Exception?>((Exception?)null);

        this.UseEffect(async () =>
        {
            try
            {
                var db = factory.CreateDbContext();
                var data = await db.Snapshots
                    .Where(s => s.CreatedAt >= startDate && s.CreatedAt <= endDate)
                    .GroupBy(s => s.CreatedAt.Date)
                    .Select(g => new
                    {
                        Date = g.Key.ToString("d MMM"),
                        SnapshotCount = g.Count()
                    })
                    .ToListAsync();

                chart.Set(data.ToLineChart(
                    e => e.Date,
                    [e => e.Sum(f => (double)f.SnapshotCount)],
                    LineChartStyles.Dashboard));
            }
            catch (Exception ex)
            {
                exception.Set(ex);
            }
        }, []);

        var card = new Card().Title("Daily Snapshot Creation Trend").Height(Size.Units(80));

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