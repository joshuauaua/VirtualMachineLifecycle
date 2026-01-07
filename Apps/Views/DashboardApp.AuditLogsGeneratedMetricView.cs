/*
The total number of audit logs generated within the selected date range.
SELECT COUNT(*) FROM audit_logs WHERE Timestamp BETWEEN @StartDate AND @EndDate
*/
namespace VirtualMachineLifecycle.Apps.Views;

public class AuditLogsGeneratedMetricView(DateTime fromDate, DateTime toDate) : ViewBase
{
    public override object Build()
    {
        var factory = UseService<VirtualMachineLifecycleContextFactory>();

        async Task<MetricRecord> CalculateAuditLogsGenerated()
        {
            await using var db = factory.CreateDbContext();

            var currentPeriodAuditLogs = await db.AuditLogs
                .Where(a => a.Timestamp >= fromDate && a.Timestamp <= toDate)
                .CountAsync();

            var periodLength = toDate - fromDate;
            var previousFromDate = fromDate.AddDays(-periodLength.TotalDays);
            var previousToDate = fromDate.AddDays(-1);

            var previousPeriodAuditLogs = await db.AuditLogs
                .Where(a => a.Timestamp >= previousFromDate && a.Timestamp <= previousToDate)
                .CountAsync();

            if (previousPeriodAuditLogs == 0)
            {
                return new MetricRecord(
                    MetricFormatted: currentPeriodAuditLogs.ToString("N0"),
                    TrendComparedToPreviousPeriod: null,
                    GoalAchieved: null,
                    GoalFormatted: null
                );
            }

            double? trend = ((double)currentPeriodAuditLogs - previousPeriodAuditLogs) / previousPeriodAuditLogs;

            var goal = previousPeriodAuditLogs * 1.1;
            double? goalAchievement = goal > 0 ? (double?)(currentPeriodAuditLogs / goal ): null;

            return new MetricRecord(
                MetricFormatted: currentPeriodAuditLogs.ToString("N0"),
                TrendComparedToPreviousPeriod: trend,
                GoalAchieved: goalAchievement,
                GoalFormatted: goal.ToString("N0")
            );
        }

        return new MetricView(
            "Audit Logs Generated",
            Icons.FileText,
            CalculateAuditLogsGenerated
        );
    }
}