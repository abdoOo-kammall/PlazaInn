using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.DTO.Payment
{
    public class PaymentRequestDto
    {
        public int AmountCents { get; set; }
        public string Currency { get; set; }
        public BillingData BillingData { get; set; }
        public List<PaymentItem> Items { get; set; }
    }
}
