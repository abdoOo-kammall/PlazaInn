namespace Shared.DTO.Dashboard
{
    public class RecentBookingDTO
    {
        public string EncodedId { get; set; } = "";
        public string BookingReference { get; set; } = "";
        public string UserName { get; set; } = "";
        public string Email { get; set; } = "";
        public int BookingType { get; set; }
        public string BookingTypeName { get; set; } = "";
        public string ItemName { get; set; } = "";
        public decimal TotalPrice { get; set; }
        public string Status { get; set; } = "";
        public string PaymentStatus { get; set; } = "";
        public DateTime CreatedAt { get; set; }
    }

    public class LowCapacityAlertDTO
    {
        public string EncodedId { get; set; } = "";
        public string ItemType { get; set; } = "";
        public string Name { get; set; } = "";
        public int Capacity { get; set; }
        public int Booked { get; set; }
        public int Remaining { get; set; }
        public DateTime? ServiceDate { get; set; }
    }

    public class RecentActivityAndAlertsDTO
    {
        public List<RecentBookingDTO> RecentBookings { get; set; } = new();
        public List<LowCapacityAlertDTO> LowCapacityAlerts { get; set; } = new();
    }
}
