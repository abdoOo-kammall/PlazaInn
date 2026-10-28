namespace Shared.DTO.Dashboard
{
    public class AdminDashboardSummaryDTO
    {
        public KpiSummaryDTO Kpis { get; set; } = new();
        public List<ServiceTypeBreakdownDTO> RevenueByServiceType { get; set; } = new();
        public List<MonthlyTrendDTO> MonthlyTrends { get; set; } = new();
        public TopPerformersDTO TopPerformers { get; set; } = new();
        public RecentActivityAndAlertsDTO OperationalInsights { get; set; } = new();
    }
}
