using Shared.DTO.Payment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PlazaCore.ServiceContract
{
   public interface  IPaymobService
    {
        bool IsLive { get; }
        string Currency { get; }

        Task<string> GetTokenAsync();
        Task<(string paymentUrl, long paymobOrderId)> CreatePaymentLinkAsync(PaymobPaymentLinkRequest request);
        Task<PaymobOrderStatusDto> GetOrderStatusAsync(long orderId);

        /// <param name="receivedHmac">The "hmac" query-string value Paymob appends to the callback URL.</param>
        /// <param name="transaction">The "obj" element of the Paymob transaction callback body.</param>
        bool ValidateHmacSignature(string receivedHmac, JsonElement transaction);

    }
}
