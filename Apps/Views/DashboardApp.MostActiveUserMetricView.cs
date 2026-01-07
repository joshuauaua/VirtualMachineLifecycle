/*
The user who generated the highest number of audit logs within the selected date range.
SELECT UserId, COUNT(*) AS LogCount FROM audit_logs WHERE Timestamp BETWEEN @StartDate AND @EndDate GROUP BY UserId ORDER BY LogCount DESC LIMIT 1
*/
namespace VirtualMachineLifecycle.Apps.Views;

public class MostActiveUserMetricView(DateTime fromDate, DateTime toDate) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<VirtualMachineLifecycleContextFactory>();

        async Task<MetricRecord> CalculateMostActiveUser()
        {
            await using var db = factory.CreateDbContext();

            var currentPeriodLogs = await db.AuditLogs
                .Where(log => log.Timestamp >= fromDate && log.Timestamp <= toDate)
                .GroupBy(log => log.UserId)
                .Select(group => new
                {
                    UserId = group.Key,
                    LogCount = group.Count()
                })
                .OrderByDescending(result => result.LogCount)
                .FirstOrDefaultAsync();

            var periodLength = toDate - fromDate;
            var previousFromDate = fromDate.AddDays(-periodLength.TotalDays);
            var previousToDate = fromDate.AddDays(-1);

            var previousPeriodLogs = await db.AuditLogs
                .Where(log => log.Timestamp >= previousFromDate && log.Timestamp <= previousToDate)
                .GroupBy(log => log.UserId)
                .Select(group => new
                {
                    UserId = group.Key,
                    LogCount = group.Count()
                })
                .OrderByDescending(result => result.LogCount)
                .FirstOrDefaultAsync();

            if (currentPeriodLogs == null)
            {
                return new MetricRecord(
                    MetricFormatted: "N/A",
                    TrendComparedToPreviousPeriod: null,
                    GoalAchieved: null,
                    GoalFormatted: null
                );
            }

            double? trend = null;
            if (previousPeriodLogs != null && previousPeriodLogs.LogCount > 0)
            {
                trend = ((double)currentPeriodLogs.LogCount - previousPeriodLogs.LogCount) / previousPeriodLogs.LogCount;
            }

            var goal = previousPeriodLogs != null ? (double?)(previousPeriodLogs.LogCount * 1.1 ): null;
            double? goalAchievement = goal.HasValue && goal > 0 ? currentPeriodLogs.LogCount / goal : null;

            var mostActiveUser = await db.Users
                .Where(user => user.Id == currentPeriodLogs.UserId)
                .Select(user => user.Username)
                .FirstOrDefaultAsync();

            return new MetricRecord(
                MetricFormatted: $"{mostActiveUser} ({currentPeriodLogs.LogCount:N0} logs)",
                TrendComparedToPreviousPeriod: trend,
                GoalAchieved: goalAchievement,
                GoalFormatted: goal?.ToString("N0")
            );
        }

        return new MetricView(
            "Most Active User",
            Icons.UserCheck,
            CalculateMostActiveUser
        );
    }
}