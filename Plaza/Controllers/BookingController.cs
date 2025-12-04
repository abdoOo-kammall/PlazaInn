using Microsoft.AspNetCore.Mvc;
using PlazaCore.ServiceContract;
using Shared.DTO.Booking;
using Shared.DTO.Payment;

namespace Plaza.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingController : ControllerBase
    {
        private readonly IBookingService _bookingService;

        public BookingController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        [HttpPost("check-availability")]
        public async Task<IActionResult> CheckAvailability([FromBody] BookingRequestDto request)
        {
            var available = await _bookingService.CheckAvailabilityAsync(request);
            return Ok(new { isAvailable = available });
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateBooking([FromBody] BookingRequestDto request)
        {
            var booking = await _bookingService.CreateBookingAsync(request);
            return Ok(booking);
        }

        [HttpGet("{encodedId}")]
        public async Task<IActionResult> GetBooking(string encodedId)
        {
            var id = Shared.Security.IdEncoder.DecodeId(encodedId);
            var booking = await _bookingService.GetBookingByIdAsync(id);
            if (booking == null) return NotFound();

            return Ok(booking);
        }

        [HttpPost("confirm-payment")]
        public async Task<IActionResult> ConfirmPayment([FromBody] PaymentConfirmationDto dto)
        {
            await _bookingService.ConfirmBookingPaymentAsync(dto.PaymentReference);
            return Ok(new { message = "Payment confirmed" });
        }
    }
}
