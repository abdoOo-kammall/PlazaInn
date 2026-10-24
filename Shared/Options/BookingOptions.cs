namespace Shared.Options
{
    public class BookingOptions
    {
        public const string SectionName = "Booking";

        public decimal TaxRate { get; set; } = 0.28m;
        public int PendingExpiryMinutes { get; set; } = 30;
        public int CleanupIntervalMinutes { get; set; } = 5;
    }
}
