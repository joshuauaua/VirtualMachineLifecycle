/*
The average number of snapshots created per virtual machine.
SELECT AVG(SnapshotCount) FROM (SELECT COUNT(*) AS SnapshotCount FROM snapshots GROUP BY VmId)
*/
namespace VirtualMachineLifecycle.Apps.Views;

public class SnapshotsPerVirtualMachineMetricView(DateTime fromDate, DateTime toDate) : ViewBase
{
    public override object Build()
    {
        var factory = UseService<VirtualMachineLifecycleContextFactory>();

        async Task<MetricRecord> CalculateSnapshotsPerVm()
        {
            await using var db = factory.CreateDbContext();

            var currentPeriodSnapshots = await db.Snapshots
                .Where(s => s.CreatedAt >= fromDate && s.CreatedAt <= toDate)
                .GroupBy(s => s.VmId)
                .Select(g => new { VmId = g.Key, SnapshotCount = g.Count() })
                .ToListAsync();

            var currentAverageSnapshots = currentPeriodSnapshots.Any()
                ? currentPeriodSnapshots.Average(g => (double)g.SnapshotCount)
                : 0.0;

            var periodLength = toDate - fromDate;
            var previousFromDate = fromDate.AddDays(-periodLength.TotalDays);
            var previousToDate = fromDate.AddDays(-1);

            var previousPeriodSnapshots = await db.Snapshots
                .Where(s => s.CreatedAt >= previousFromDate && s.CreatedAt <= previousToDate)
                .GroupBy(s => s.VmId)
                .Select(g => new { VmId = g.Key, SnapshotCount = g.Count() })
                .ToListAsync();

            var previousAverageSnapshots = previousPeriodSnapshots.Any()
                ? previousPeriodSnapshots.Average(g => (double)g.SnapshotCount)
                : 0.0;

            double? trend = previousAverageSnapshots > 0
                ? (double?)((currentAverageSnapshots - previousAverageSnapshots) / previousAverageSnapshots
)                : null;

            var goal = previousAverageSnapshots * 1.1;
            double? goalAchievement = goal > 0 ? currentAverageSnapshots / goal : null;

            return new MetricRecord(
                MetricFormatted: currentAverageSnapshots.ToString("N2"),
                TrendComparedToPreviousPeriod: trend,
                GoalAchieved: goalAchievement,
                GoalFormatted: goal.ToString("N2")
            );
        }

        return new MetricView(
            "Snapshots Per Virtual Machine",
            Icons.Layers,
            CalculateSnapshotsPerVm
        );
    }
}