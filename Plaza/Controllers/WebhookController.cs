using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Plaza.Exceptions;
using PlazaCore.ServiceContract;

namespace Plaza.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WebhookController : ControllerBase
    {
        private readonly IBookingService _bookingService;
        private readonly IPaymobService _paymobService;
        private readonly ILogger<WebhookController> _logger;

        public WebhookController(IBookingService bookingService, IPaymobService paymobService, ILogger<WebhookController> logger)
        {
            _bookingService = bookingService;
            _paymobService = paymobService;
            _logger = logger;
        }

        [HttpPost("callback")]
        public async Task<IActionResult> PaymobCallback([FromBody] JsonElement payload, [FromQuery] string? hmac)
        {
            if (payload.ValueKind != JsonValueKind.Object
                || !payload.TryGetProperty("obj", out var transaction)
                || transaction.ValueKind != JsonValueKind.Object)
            {
                return BadRequest();
            }

            if (payload.TryGetProperty("type", out var type)
                && type.ValueKind == JsonValueKind.String
                && !string.Equals(type.GetString(), "TRANSACTION", StringComparison.OrdinalIgnoreCase))
            {
                return Ok();
            }

            // 1. اتأكد إن الريكوست جاي فعلاً من Paymob (HMAC)
            if (string.IsNullOrWhiteSpace(hmac) || !_paymobService.ValidateHmacSignature(hmac, transaction))
            {
                _logger.LogWarning("Rejected Paymob callback with missing or invalid HMAC.");
                return Unauthorized();
            }

            if (!IsTrue(transaction, "success") || IsTrue(transaction, "pending"))
                return Ok();

            if (!transaction.TryGetProperty("order", out var order)
                || !order.TryGetProperty("id", out var orderIdElement)
                || !orderIdElement.TryGetInt64(out var paymobOrderId)
                || !transaction.TryGetProperty("amount_cents", out var amountElement)
                || !amountElement.TryGetInt64(out var paidAmountCents))
            {
                return BadRequest();
            }

            try
            {
                // 2. اسأل Paymob تاني عن حالة الأوردر بدل ما نصدق الـ payload بس
                var orderStatus = await _paymobService.GetOrderStatusAsync(paymobOrderId);

                if (orderStatus.IsPaid)
                {
                    // 3. أكد الحجز (idempotent + التحقق من المبلغ ورقم الأوردر)
                    await _bookingService.ConfirmBookingPaymentAsync(paymobOrderId, paidAmountCents, orderStatus.merchant_order_id);
                }

                return Ok();
            }
            catch (PaymentGatewayException)
            {
                // فشل في التحقق من حالة الطلب من Paymob، نرجع 400 عشان نقدر نراقب المشكلة
                return BadRequest();
            }
        }

        private static bool IsTrue(JsonElement element, string property) =>
            element.TryGetProperty(property, out var value) && value.ValueKind == JsonValueKind.True;
    }
}
