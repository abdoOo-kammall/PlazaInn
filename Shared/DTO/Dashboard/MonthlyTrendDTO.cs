namespace Shared.DTO.Dashboard
{
    public class MonthlyTrendDTO
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string PeriodLabel { get; set; } = "";
        public int BookingsCount { get; set; }
        public decimal ConfirmedRevenue { get; set; }
    }
}
