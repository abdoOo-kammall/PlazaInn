using Shared.DTO.Payment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlazaCore.ServiceContract
{
   public interface  IPaymobService
    {
        Task<string> GetTokenAsync(bool isLive);
        Task<string> CreatePaymentLinkAsync(PaymentRequestDto request, bool isLive);

    }
}
