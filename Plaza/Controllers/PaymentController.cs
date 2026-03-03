using Azure.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PlazaCore.ServiceContract;
using PlazaService.BookingService;
using Shared.DTO.Payment;
using Shared.Security;

namespace Plaza.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymobService _paymob;
        private readonly IBookingService _bookingService;

        public PaymentController(IPaymobService paymobService , IBookingService bookingService)
        {
            this._paymob = paymobService;
            this._bookingService = bookingService;
        }
        //[HttpPost("test")]
        //public async Task<ActionResult<string>> testPayment(PaymentRequestDto requestDto)
        //{

        //    //var url = await _paymob.CreatePaymentLinkAsync(requestDto, false);
        //    var (paymentUrl, paymobOrderId) =
        //                                         await _paymob.CreatePaymentLinkAsync(requestDto, false);

        //    int bookingId = IdEncoder.DecodeId(requestDto.EncodedBookingId);

        //    await _bookingService.AttachPaymentReferenceAsync(
        //        bookingId,
        //        paymobOrderId.ToString()
        //    );

        //    return Ok(new { paymentUrl });

        //}
        [HttpPost("test")]
        public async Task<ActionResult<string>> testPayment(PaymentRequestDto requestDto)
        {
            try
            {
                var (paymentUrl, paymobOrderId) = await _paymob.CreatePaymentLinkAsync(requestDto, false);

                int bookingId = IdEncoder.DecodeId(requestDto.EncodedBookingId);
                await _bookingService.AttachPaymentReferenceAsync(bookingId, paymobOrderId.ToString());

                return Ok(new { paymentUrl = paymentUrl  , paymobOrderId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message, stack = ex.StackTrace });
            }
        }
        [HttpGet("check-order/{paymobOrderId}")]
        public async Task<IActionResult> CheckPaymobOrder(long paymobOrderId)
        {
            try
            {
                // 1. استعلم عن حالة الدفع من Paymob
                var orderStatus = await _paymob.GetOrderStatusAsync(paymobOrderId);

                // 2. لو الدفع ناجح، حدث الحجز
                if (orderStatus.payment_status == "PAID" && orderStatus.is_paid)
                {
                    await _bookingService.ConfirmBookingPaymentAsync(paymobOrderId.ToString());
                }

                // 3. رجع النتيجة كاملة للعميل
                return Ok(orderStatus);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

    }
}
