using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Plaza.Exceptions;
using Plaza.Extensions;
using PlazaCore.ServiceContract;
using Shared.DTO.Payment;
using Shared.Security;

namespace Plaza.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymobService _paymob;
        private readonly IBookingService _bookingService;

        public PaymentController(IPaymobService paymobService , IBookingService bookingService)
        {
            this._paymob = paymobService;
            this._bookingService = bookingService;
        }

        // "test" is kept as an alias so existing frontend calls keep working.
        [HttpPost("initiate")]
        [HttpPost("test")]
        public async Task<IActionResult> InitiatePayment([FromBody] PaymentRequestDto requestDto)
        {
            var userId = User.GetUserId();
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized(new { error = "User id claim is missing in token." });

            var isAdmin = User.IsAdmin();
            int bookingId = IdEncoder.DecodeId(requestDto.EncodedBookingId);

            // 1. المبلغ والعملة بيتحسبوا من الحجز على السيرفر، مش من الريكوست
            var paymentInfo = await _bookingService.PrepareBookingForPaymentAsync(bookingId, userId, isAdmin);

            var billing = requestDto.BillingData ?? new BillingData();
            billing.Name = string.IsNullOrWhiteSpace(billing.Name) ? paymentInfo.CustomerName : billing.Name;
            billing.Email = string.IsNullOrWhiteSpace(billing.Email) ? paymentInfo.CustomerEmail : billing.Email;
            billing.Phone = string.IsNullOrWhiteSpace(billing.Phone) ? paymentInfo.CustomerPhone ?? string.Empty : billing.Phone;

            // 2. اعمل لينك الدفع على Paymob
            var (paymentUrl, paymobOrderId) = await _paymob.CreatePaymentLinkAsync(new PaymobPaymentLinkRequest
            {
                AmountCents = paymentInfo.AmountCents,
                Currency = _paymob.Currency,
                MerchantOrderId = paymentInfo.MerchantOrderId,
                BillingData = billing
            });

            // 3. اربط رقم أوردر Paymob بالحجز عشان الـ Webhook يلاقيه
            await _bookingService.AttachPaymentReferenceAsync(
                bookingId, paymobOrderId.ToString(), paymentInfo.MerchantOrderId);

            return Ok(new { paymentUrl, paymobOrderId, amountCents = paymentInfo.AmountCents, currency = _paymob.Currency });
        }

        [HttpGet("check-order/{paymobOrderId}")]
        public async Task<IActionResult> CheckPaymobOrder(long paymobOrderId)
        {
            var userId = User.GetUserId();
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized(new { error = "User id claim is missing in token." });

            // 1. استعلم عن حالة الدفع من Paymob (الـ merchant_order_id بيلاقي الحجز لو رقم الأوردر اتبدل بمحاولة دفع أحدث)
            var orderStatus = await _paymob.GetOrderStatusAsync(paymobOrderId);

            await _bookingService.EnsureCanAccessPaymentAsync(
                paymobOrderId.ToString(), orderStatus.merchant_order_id, userId, User.IsAdmin());

            // 2. لو الدفع ناجح، أكد الحجز (بعد التحقق من المبلغ ورقم الأوردر)
            var bookingConfirmed = false;
            if (orderStatus.IsPaid)
            {
                var paidCents = orderStatus.paid_amount_cents > 0 ? orderStatus.paid_amount_cents : orderStatus.amount_cents;
                bookingConfirmed = await _bookingService.ConfirmBookingPaymentAsync(
                    paymobOrderId, paidCents, orderStatus.merchant_order_id);
            }

            // 3. رجع للعميل حالة الدفع والحجز بس
            return Ok(new
            {
                orderStatus.id,
                orderStatus.payment_status,
                is_paid = orderStatus.IsPaid,
                orderStatus.amount_cents,
                orderStatus.currency,
                booking_confirmed = bookingConfirmed
            });
        }

    }
}
