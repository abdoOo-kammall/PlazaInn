using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.DTO.Payment
{
    public class PaymentRequestDto
    {
        [Required]
        public string EncodedBookingId { get; set; } = null!;

        // Ignored: amount, currency and items are always derived server-side from the booking.
        public int? AmountCents { get; set; }
        public string? Currency { get; set; }
        public List<PaymentItem>? Items { get; set; }

        public BillingData? BillingData { get; set; }
    }
}
