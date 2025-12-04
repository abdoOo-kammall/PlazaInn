using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.DTO.Payment
{
    public class PaymentLinkResponse
    {
        public string shorten_url { get; set; }
        public string client_url { get; set; }
    }
}
