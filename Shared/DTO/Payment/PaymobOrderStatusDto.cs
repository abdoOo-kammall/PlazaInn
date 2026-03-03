using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.DTO.Payment
{
    public class PaymobOrderStatusDto
    {
        public long id { get; set; }
        public string payment_status { get; set; } = "";
        public bool is_paid { get; set; }
        public string payment_reference { get; set; } = "";
        public long amount_cents { get; set; }
        public string currency { get; set; } = "";
    }

}
