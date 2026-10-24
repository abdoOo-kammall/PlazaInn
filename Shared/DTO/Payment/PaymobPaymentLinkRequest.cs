namespace Shared.DTO.Payment
{
    public class PaymobPaymentLinkRequest
    {
        public long AmountCents { get; set; }
        public string Currency { get; set; } = "EGP";
        public string MerchantOrderId { get; set; } = null!;
        public BillingData BillingData { get; set; } = null!;
    }
}
