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
            // 1) Get token correctly
            var token = await GetTokenAsync(isLive);

            // 2) Load integration ID correctly
            var integrationId = !isLive
                ? int.Parse(_config["PaymobTest:IntegrationIdTest"])
                : int.Parse(_config["Paymob:IntegrationIdLive"]);

            // 3) Build body correctly
            var jsonBody = new
            {
                auth_token = token,
                amount_cents = request.AmountCents,
                currency = request.Currency,
                payment_methods = new[] { integrationId },
                billing_data = request.BillingData,
                items = request.Items,
                is_live=isLive
            };

            var response = await _httpClient.PostAsJsonAsync(
                "https://accept.paymob.com/api/ecommerce/payment-links",
                jsonBody);
            var raw = await response.Content.ReadAsStringAsync();
            Console.WriteLine("RAW RESPONSE FROM PAYMOB:");
            Console.WriteLine(raw);

            var linkData = await response.Content.ReadFromJsonAsync<PaymentLinkResponse>();

            if (linkData?.client_url == null)
                //throw new Exception("Failed To Loading PaymentUrl");
                throw new Exception("Paymob Error: " + raw);


            return linkData.client_url;
        }


        public async Task<string> GetTokenAsync(bool isLive)
        {
            var apiKey = !isLive
                ? _config["PaymobTest:ApiKeyTest"]
                : _config["Paymob:ApiKeyLive"];

            var jsonBody = new { api_key = apiKey };

            var response = await _httpClient.PostAsJsonAsync(
                "https://accept.paymob.com/api/auth/tokens",
                jsonBody);

            var data = await response.Content.ReadFromJsonAsync<AuthResponse>();

            if (data?.Token == null)
                throw new Exception("Failed to retrieve Paymob token.");

            return data.Token;
        }

    }
}
