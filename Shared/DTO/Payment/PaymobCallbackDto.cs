using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.DTO.Payment
{
    public class PaymobCallbackDto
    {
        public bool success { get; set; }
        public PaymobOrderDto order { get; set; } = null!;
    }

    public class PaymobOrderDto
    {
        public long id { get; set; } // 🔥 ده Paymob Order ID
    }

}
