namespace Shared.DTO.Dashboard
{
    public class ServiceTypeBreakdownDTO
    {
        public int BookingType { get; set; }
        public string BookingTypeName { get; set; } = "";
        public int BookingsCount { get; set; }
        public int ConfirmedCount { get; set; }
        public decimal Revenue { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal RevenueSharePercentage { get; set; }
    }
}
