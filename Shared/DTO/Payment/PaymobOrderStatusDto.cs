using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Shared.DTO.Payment
{
    public class PaymobOrderStatusDto
    {
        public long id { get; set; }
        public string payment_status { get; set; } = "";
        [JsonPropertyName("is_paid")]
        public bool is_paid { get; set; }
        public string payment_reference { get; set; } = "";
        public long amount_cents { get; set; }
        public long paid_amount_cents { get; set; }
        public string? merchant_order_id { get; set; }
        public string currency { get; set; } = "";

        [JsonIgnore]
        public bool IsPaid => is_paid || string.Equals(payment_status, "PAID", StringComparison.OrdinalIgnoreCase);
    }

}
