/*
The total number of users registered in the system.
SELECT COUNT(*) FROM users
*/
namespace VirtualMachineLifecycle.Apps.Views;

public class TotalUsersMetricView(DateTime fromDate, DateTime toDate) : ViewBase
{
    public override object Build()
    {
        var factory = UseService<VirtualMachineLifecycleContextFactory>();

        async Task<MetricRecord> CalculateTotalUsers()
        {
            await using var db = factory.CreateDbContext();

            var currentPeriodUsers = await db.Users
                .Where(u => u.CreatedAt >= fromDate && u.CreatedAt <= toDate)
                .CountAsync();

            var periodLength = toDate - fromDate;
            var previousFromDate = fromDate.AddDays(-periodLength.TotalDays);
            var previousToDate = fromDate.AddDays(-1);

            var previousPeriodUsers = await db.Users
                .Where(u => u.CreatedAt >= previousFromDate && u.CreatedAt <= previousToDate)
                .CountAsync();

            if (previousPeriodUsers == 0)
            {
                return new MetricRecord(
                    MetricFormatted: currentPeriodUsers.ToString("N0"),
                    TrendComparedToPreviousPeriod: null,
                    GoalAchieved: null,
                    GoalFormatted: null
                );
            }

            double? trend = ((double)currentPeriodUsers - previousPeriodUsers) / previousPeriodUsers;

            var goal = previousPeriodUsers * 1.1;
            double? goalAchievement = goal > 0 ? (double?)(currentPeriodUsers / goal ): null;

            return new MetricRecord(
                MetricFormatted: currentPeriodUsers.ToString("N0"),
                TrendComparedToPreviousPeriod: trend,
                GoalAchieved: goalAchievement,
                GoalFormatted: goal.ToString("N0")
            );
        }

        return new MetricView(
            "Total Users",
            Icons.Users,
            CalculateTotalUsers
        );
    }
}