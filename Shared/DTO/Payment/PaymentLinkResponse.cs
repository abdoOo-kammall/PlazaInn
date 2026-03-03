using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.DTO.Payment
{
    public class PaymentLinkResponse
    {
        public string client_url { get; set; }
        public long order { get; set; }
    }

    public class PaymobOrder
    {
        public long id { get; set; } // 🔥 ده أهم حاجة
    }

}
