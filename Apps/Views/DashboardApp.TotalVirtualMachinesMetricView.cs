/*
The total number of virtual machines currently managed by the system.
SELECT COUNT(*) FROM vms
*/
namespace VirtualMachineLifecycle.Apps.Views;

public class TotalVirtualMachinesMetricView(DateTime fromDate, DateTime toDate) : ViewBase
{
    public override object Build()
    {
        var factory = UseService<VirtualMachineLifecycleContextFactory>();

        async Task<MetricRecord> CalculateTotalVirtualMachines()
        {
            await using var db = factory.CreateDbContext();

            var currentPeriodVms = await db.Vms
                .Where(vm => vm.CreatedAt >= fromDate && vm.CreatedAt <= toDate)
                .CountAsync();

            var periodLength = toDate - fromDate;
            var previousFromDate = fromDate.AddDays(-periodLength.TotalDays);
            var previousToDate = fromDate.AddDays(-1);

            var previousPeriodVms = await db.Vms
                .Where(vm => vm.CreatedAt >= previousFromDate && vm.CreatedAt <= previousToDate)
                .CountAsync();

            if (previousPeriodVms == 0)
            {
                return new MetricRecord(
                    MetricFormatted: currentPeriodVms.ToString("N0"),
                    TrendComparedToPreviousPeriod: null,
                    GoalAchieved: null,
                    GoalFormatted: null
                );
            }

            double? trend = ((double)currentPeriodVms - previousPeriodVms) / previousPeriodVms;
            var goal = previousPeriodVms * 1.1;
            double? goalAchievement = goal > 0 ? (double?)(currentPeriodVms / goal ): null;

            return new MetricRecord(
                MetricFormatted: currentPeriodVms.ToString("N0"),
                TrendComparedToPreviousPeriod: trend,
                GoalAchieved: goalAchievement,
                GoalFormatted: goal.ToString("N0")
            );
        }

        return new MetricView(
            "Total Virtual Machines",
            Icons.Server,
            CalculateTotalVirtualMachines
        );
    }
}