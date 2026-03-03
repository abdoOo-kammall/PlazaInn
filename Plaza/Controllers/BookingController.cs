using Microsoft.AspNetCore.Mvc;
using PlazaCore.ServiceContract;
using Shared.DTO.Booking;
using Shared.Security;

namespace PlazaAPI.Controllers
{
    [ApiController]
    [Route("api/bookings")]
    public class BookingController : ControllerBase
    {
        private readonly IBookingService _bookingService;

        public BookingController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateBooking([FromBody] BookingRequestDto request)
        {
            var booking = await _bookingService.CreateBookingAsync(request);
            return Ok(new { success = true, data = booking });
        }

        [HttpPost("confirm-payment")]
        public async Task<IActionResult> ConfirmPayment([FromQuery] string paymentRef)
        {
            if (string.IsNullOrWhiteSpace(paymentRef))
                return BadRequest(new { success = false, message = "Payment reference is required" });

            await _bookingService.ConfirmBookingPaymentAsync(paymentRef);
            return Ok(new { success = true, message = "Payment confirmed successfully" });
        }
    }

}
