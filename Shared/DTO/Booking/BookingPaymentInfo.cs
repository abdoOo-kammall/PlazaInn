namespace Shared.DTO.Booking
{
    public class BookingPaymentInfo
    {
        public int BookingId { get; set; }
        public long AmountCents { get; set; }
        public string MerchantOrderId { get; set; } = null!;
        public string CustomerName { get; set; } = null!;
        public string CustomerEmail { get; set; } = null!;
        public string? CustomerPhone { get; set; }
    }
}
