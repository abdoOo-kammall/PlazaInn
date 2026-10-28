namespace Shared.DTO.Dashboard
{
    /// <summary>
    /// Active/total pair. Hotels and rooms have no IsActive flag, so active equals total.
    /// </summary>
    public class CatalogCountDTO
    {
        public int Active { get; set; }
        public int Total { get; set; }
    }

    public class CatalogCountsDTO
    {
        public CatalogCountDTO Hotels { get; set; } = new();
        public CatalogCountDTO Rooms { get; set; } = new();
        public CatalogCountDTO Trips { get; set; } = new();
        public CatalogCountDTO Transports { get; set; } = new();
        public CatalogCountDTO Events { get; set; } = new();
        public CatalogCountDTO Packages { get; set; } = new();
    }

    public class KpiSummaryDTO
    {
        public decimal TotalConfirmedRevenue { get; set; }
        public decimal TotalTaxCollected { get; set; }
        public decimal PendingRevenue { get; set; }
        public int TotalBookings { get; set; }
        public int ConfirmedBookings { get; set; }
        public int PendingBookings { get; set; }
        public int CancelledBookings { get; set; }
        public int ExpiredBookings { get; set; }
        public int TotalUsers { get; set; }
        public CatalogCountsDTO CatalogCounts { get; set; } = new();
    }
}
