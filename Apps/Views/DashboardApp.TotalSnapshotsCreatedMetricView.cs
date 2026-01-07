/*
The total number of snapshots created within the selected date range.
SELECT COUNT(*) FROM snapshots WHERE CreatedAt BETWEEN @StartDate AND @EndDate
*/
namespace VirtualMachineLifecycle.Apps.Views;

public class TotalSnapshotsCreatedMetricView(DateTime fromDate, DateTime toDate) : ViewBase
{
    public override object Build()
    {
        var factory = UseService<VirtualMachineLifecycleContextFactory>();
        
        async Task<MetricRecord> CalculateTotalSnapshotsCreated()
        {
            await using var db = factory.CreateDbContext();
            
            var currentPeriodSnapshots = await db.Snapshots
                .Where(s => s.CreatedAt >= fromDate && s.CreatedAt <= toDate)
                .CountAsync();
                
            var periodLength = toDate - fromDate;
            var previousFromDate = fromDate.AddDays(-periodLength.TotalDays);
            var previousToDate = fromDate.AddDays(-1);
            
            var previousPeriodSnapshots = await db.Snapshots
                .Where(s => s.CreatedAt >= previousFromDate && s.CreatedAt <= previousToDate)
                .CountAsync();

            if (previousPeriodSnapshots == 0)
            {
                return new MetricRecord(
                    MetricFormatted: currentPeriodSnapshots.ToString("N0"),
                    TrendComparedToPreviousPeriod: null,
                    GoalAchieved: null,
                    GoalFormatted: null
                );
            }
            
            double? trend = ((double)currentPeriodSnapshots - previousPeriodSnapshots) / previousPeriodSnapshots;
            
            var goal = previousPeriodSnapshots * 1.1;
            double? goalAchievement = goal > 0 ? (double?)(currentPeriodSnapshots / goal ): null;
            
            return new MetricRecord(
                MetricFormatted: currentPeriodSnapshots.ToString("N0"),
                TrendComparedToPreviousPeriod: trend,
                GoalAchieved: goalAchievement,
                GoalFormatted: goal.ToString("N0")
            );
        }

        return new MetricView(
            "Total Snapshots Created",
            Icons.Camera,
            CalculateTotalSnapshotsCreated
        );
    }
}