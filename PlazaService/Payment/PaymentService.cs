using Microsoft.Extensions.Configuration;
using PlazaCore.ServiceContract;
using Shared.DTO.Payment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace PlazaService.Payment
{
    public class PaymentService : IPaymobService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;

        public PaymentService(HttpClient httpClient , IConfiguration config)
        {
            this._httpClient = httpClient;
            _config = config;
        }
        public async Task<string> CreatePaymentLinkAsync(PaymentRequestDto request, bool isLive)
        {
            var token = GetTokenAsync(isLive);
            var integrationId = !isLive ? _config["IntegrationIdTest"] : _config["IntegrationIdLive"];
            var jsonBody = new
            {
                amount_cents = request.AmountCents,
                currency = request.Currency,
                integration_id = integrationId,
                billing_data = request.BillingData,
                items = request.Items
            };
            var response = await _httpClient.PostAsJsonAsync("https://accept.paymob.com/api/ecommerce/payment-links" ,jsonBody);
            var tokenUrl = await response.Content.ReadFromJsonAsync<PaymentLinkResponse>();
            if (tokenUrl.PaymentUrl == null) {
                throw new Exception("Failed To Loading PaymentUrl");
            }
            return tokenUrl.PaymentUrl;

        }

        public async Task<string> GetTokenAsync(bool isLive)
        {
            var apiKey = !isLive ? _config["ApiKeyTest"] : _config["ApiKeyLive"];
            var jsonBody = new { apapi_key = apiKey };
            var response = await _httpClient.PostAsJsonAsync("https://accept.paymob.com/api/auth/tokens" , jsonBody);
            var data = await response.Content.ReadFromJsonAsync<AuthResponse>();
            if (data.Token == null)
                {
                throw new Exception("Failed to retrieve Paymob token.");
                }
            return data.Token;
        }
    }
}
