using VirtualMachineLifecycle.Apps.Views;

namespace VirtualMachineLifecycle.Apps;

[App(icon: Icons.ChartBar, path: ["Apps"])]
public class DashboardApp : ViewBase
{
    public override object? Build()
    {
        var range = this.UseState(() => (fromDate:DateTime.Today.Date.AddDays(-30), toDate:DateTime.Today.Date));
        
        var header = Layout.Horizontal().Align(Align.Right)
                    | range.ToDateRangeInput();
        
        var fromDate = range.Value.fromDate;
        var toDate = range.Value.toDate;
        
        var metrics =
                Layout.Grid().Columns(4)
| new TotalVirtualMachinesMetricView(fromDate, toDate).Key(fromDate, toDate)| new ActiveVirtualMachinesMetricView(fromDate, toDate).Key(fromDate, toDate)| new TotalSnapshotsCreatedMetricView(fromDate, toDate).Key(fromDate, toDate)| new TotalUsersMetricView(fromDate, toDate).Key(fromDate, toDate)| new SnapshotsPerVirtualMachineMetricView(fromDate, toDate).Key(fromDate, toDate)| new AuditLogsGeneratedMetricView(fromDate, toDate).Key(fromDate, toDate)| new MostActiveUserMetricView(fromDate, toDate).Key(fromDate, toDate)| new TotalActiveVirtualMachinesMetricView(fromDate, toDate).Key(fromDate, toDate)            ;

        var charts =
                Layout.Grid().Columns(3)
| new DailyVmCreationTrendLineChartView(fromDate, toDate).Key(fromDate, toDate)| new VmStatusDistributionPieChartView(fromDate, toDate).Key(fromDate, toDate)| new DailySnapshotCreationTrendLineChartView(fromDate, toDate).Key(fromDate, toDate)| new UserActivityByAuditLogsLineChartView(fromDate, toDate).Key(fromDate, toDate)| new ProviderVmDistributionPieChartView(fromDate, toDate).Key(fromDate, toDate)| new DailyVmUpdatesLineChartView(fromDate, toDate).Key(fromDate, toDate)            ;

        return Layout.Horizontal().Align(Align.Center) | 
               new HeaderLayout(header, Layout.Vertical() 
                            | metrics
                            | charts
                ).Width(Size.Full().Max(300));
    }
}