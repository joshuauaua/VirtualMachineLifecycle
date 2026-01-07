/*
The total number of virtual machines that are currently active, representing the core value delivered to customers.
SELECT COUNT(*) FROM vms WHERE VmStatus.DescriptionText = 'Active'
*/
namespace VirtualMachineLifecycle.Apps.Views;

public class TotalActiveVirtualMachinesMetricView(DateTime fromDate, DateTime toDate) : ViewBase
{
    public override object Build()
    {
        var factory = UseService<VirtualMachineLifecycleContextFactory>();

        async Task<MetricRecord> CalculateTotalActiveVirtualMachines()
        {
            await using var db = factory.CreateDbContext();

            var currentPeriodActiveVms = await db.Vms
                .Where(vm => vm.VmStatus.DescriptionText == "Active" && vm.CreatedAt >= fromDate && vm.CreatedAt <= toDate)
                .CountAsync();

            var periodLength = toDate - fromDate;
            var previousFromDate = fromDate.AddDays(-periodLength.TotalDays);
            var previousToDate = fromDate.AddDays(-1);

            var previousPeriodActiveVms = await db.Vms
                .Where(vm => vm.VmStatus.DescriptionText == "Active" && vm.CreatedAt >= previousFromDate && vm.CreatedAt <= previousToDate)
                .CountAsync();

            if (previousPeriodActiveVms == 0)
            {
                return new MetricRecord(
                    MetricFormatted: currentPeriodActiveVms.ToString("N0"),
                    TrendComparedToPreviousPeriod: null,
                    GoalAchieved: null,
                    GoalFormatted: null
                );
            }

            double? trend = ((double)currentPeriodActiveVms - previousPeriodActiveVms) / previousPeriodActiveVms;

            var goal = previousPeriodActiveVms * 1.1;
            double? goalAchievement = goal > 0 ? (double?)(currentPeriodActiveVms / goal ): null;

            return new MetricRecord(
                MetricFormatted: currentPeriodActiveVms.ToString("N0"),
                TrendComparedToPreviousPeriod: trend,
                GoalAchieved: goalAchievement,
                GoalFormatted: goal.ToString("N0")
            );
        }

        return new MetricView(
            "Total Active Virtual Machines",
            Icons.Star,
            CalculateTotalActiveVirtualMachines
        );
    }
}