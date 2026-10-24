using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Plaza.Exceptions;
using PlazaCore.ServiceContract;
using Shared.DTO.Payment;
using System;
using System.Globalization;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace PlazaService.Payment
{
    public class PaymentService : IPaymobService
    {
        private const string BaseUrl = "https://accept.paymob.com/api";

        // Field order mandated by Paymob for the "transaction processed" callback HMAC.
        private static readonly string[] TransactionHmacFields =
        {
            "amount_cents",
            "created_at",
            "currency",
            "error_occured",
            "has_parent_transaction",
            "id",
            "integration_id",
            "is_3d_secure",
            "is_auth",
            "is_capture",
            "is_refunded",
            "is_standalone_payment",
            "is_voided",
            "order.id",
            "owner",
            "pending",
            "source_data.pan",
            "source_data.sub_type",
            "source_data.type",
            "success"
        };

        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;
        private readonly ILogger<PaymentService> _logger;

        public PaymentService(HttpClient httpClient, IConfiguration config, ILogger<PaymentService> logger)
        {
            _httpClient = httpClient;
            _config = config;
            _logger = logger;
        }

        public bool IsLive => _config.GetValue("Paymob:IsLive", false);

        public string Currency => _config["Paymob:Currency"] is { Length: > 0 } currency ? currency : "EGP";

        private string? ApiKey => IsLive ? _config["Paymob:ApiKeyLive"] : _config["PaymobTest:ApiKeyTest"];

        private int IntegrationId => IsLive
            ? _config.GetValue("Paymob:IntegrationIdLive", 0)
            : _config.GetValue("PaymobTest:IntegrationIdTest", 0);

        private string? HmacSecret => IsLive ? _config["Paymob:HmacLive"] : _config["PaymobTest:HmacTest"];

        public async Task<(string paymentUrl, long paymobOrderId)> CreatePaymentLinkAsync(PaymobPaymentLinkRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            if (request.AmountCents <= 0)
                throw new PaymentGatewayException("Amount must be greater than zero.");

            if (request.BillingData == null)
                throw new PaymentGatewayException("Billing data is required.");

            var integrationId = IntegrationId;
            if (integrationId <= 0)
                throw new PaymentGatewayException("Paymob integration ID is not configured correctly.");

            var token = await GetTokenAsync();

            var jsonBody = new
            {
                auth_token = token,
                amount_cents = request.AmountCents,
                currency = request.Currency,
                payment_methods = new[] { integrationId },
                billing_data = request.BillingData,
                items = Array.Empty<object>(),
                merchant_order_id = request.MerchantOrderId,
                is_live = IsLive
            };

            var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/ecommerce/payment-links", jsonBody);
            var raw = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to create Paymob payment link. StatusCode: {StatusCode}, Response: {Response}",
                    response.StatusCode, raw);

                throw new PaymentGatewayException("Failed to create payment link with Paymob.");
            }

            var linkData = JsonSerializer.Deserialize<PaymentLinkResponse>(raw, JsonOptions);

            if (linkData?.client_url == null || linkData.order == 0)
            {
                _logger.LogError("Invalid Paymob response while creating payment link. Raw: {Raw}", raw);
                throw new PaymentGatewayException("Invalid Paymob response while creating payment link.");
            }

            return (linkData.client_url, linkData.order);
        }

        public async Task<PaymobOrderStatusDto> GetOrderStatusAsync(long orderId)
        {
            var token = await GetTokenAsync();

            var request = new HttpRequestMessage(HttpMethod.Get, $"{BaseUrl}/ecommerce/orders/{orderId}?token={Uri.EscapeDataString(token)}");

            var response = await _httpClient.SendAsync(request);
            var raw = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to retrieve Paymob order status for OrderId {OrderId}. StatusCode: {StatusCode}, Response: {Response}",
                    orderId, response.StatusCode, raw);

                throw new PaymentGatewayException("Failed to retrieve Paymob order status.");
            }

            var orderData = JsonSerializer.Deserialize<PaymobOrderStatusDto>(raw, JsonOptions);
            if (orderData == null)
            {
                _logger.LogError("Invalid Paymob response while retrieving order status. Raw: {Raw}", raw);
                throw new PaymentGatewayException("Invalid Paymob response while retrieving order status.");
            }

            return orderData;
        }

        public async Task<string> GetTokenAsync()
        {
            var apiKey = ApiKey;
            if (string.IsNullOrWhiteSpace(apiKey))
                throw new PaymentGatewayException("Paymob API key is not configured.");

            var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/auth/tokens", new { api_key = apiKey });
            var raw = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to retrieve Paymob token. StatusCode: {StatusCode}, Response: {Response}",
                    response.StatusCode, raw);
                throw new PaymentGatewayException("Failed to retrieve Paymob token.");
            }

            var data = JsonSerializer.Deserialize<AuthResponse>(raw, JsonOptions);

            if (data?.Token == null)
            {
                _logger.LogError("Paymob token response did not contain a token.");
                throw new PaymentGatewayException("Failed to retrieve Paymob token.");
            }

            return data.Token;
        }

        public bool ValidateHmacSignature(string receivedHmac, JsonElement transaction)
        {
            var secret = HmacSecret;
            if (string.IsNullOrWhiteSpace(secret))
            {
                _logger.LogError("Paymob HMAC secret is not configured; rejecting callback.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(receivedHmac) || transaction.ValueKind != JsonValueKind.Object)
                return false;

            var concatenated = new StringBuilder();
            foreach (var path in TransactionHmacFields)
                concatenated.Append(GetHmacValue(transaction, path));

            using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(secret));
            var computed = Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(concatenated.ToString()))).ToLowerInvariant();

            return CryptographicOperations.FixedTimeEquals(
                Encoding.ASCII.GetBytes(computed),
                Encoding.ASCII.GetBytes(receivedHmac.Trim().ToLowerInvariant()));
        }

        private static string GetHmacValue(JsonElement root, string path)
        {
            var current = root;
            foreach (var segment in path.Split('.'))
            {
                if (current.ValueKind != JsonValueKind.Object || !current.TryGetProperty(segment, out current))
                    return string.Empty;
            }

            return current.ValueKind switch
            {
                JsonValueKind.True => "true",
                JsonValueKind.False => "false",
                JsonValueKind.Null or JsonValueKind.Undefined => string.Empty,
                JsonValueKind.String => current.GetString() ?? string.Empty,
                JsonValueKind.Number => current.GetRawText(),
                _ => current.GetRawText()
            };
        }
    }
}
