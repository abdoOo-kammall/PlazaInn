//using Microsoft.AspNetCore.Mvc;
//using PlazaCore.ServiceContract;
//using Shared.DTO.Booking;
//using Shared.DTO.Payment;

//namespace Plaza.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class BookingController : ControllerBase
//    {
//        private readonly IBookingService _bookingService;

//        public BookingController(IBookingService bookingService)
//        {
//            _bookingService = bookingService;
//        }

//        [HttpPost("check-availability")]
//        public async Task<IActionResult> CheckAvailability([FromBody] BookingRequestDto request)
//        {
//            var available = await _bookingService.CheckAvailabilityAsync(request);
//            return Ok(new { isAvailable = available });
//        }

//        [HttpPost("create")]
//        public async Task<IActionResult> CreateBooking([FromBody] BookingRequestDto request)
//        {
//            var booking = await _bookingService.CreateBookingAsync(request);
//            return Ok(booking);
//        }

//        [HttpGet("{encodedId}")]
//        public async Task<IActionResult> GetBooking(string encodedId)
//        {
//            var id = Shared.Security.IdEncoder.DecodeId(encodedId);
//            var booking = await _bookingService.GetBookingByIdAsync(id);
//            if (booking == null) return NotFound();

//            return Ok(booking);
//        }

//        [HttpPost("confirm-payment")]
//        public async Task<IActionResult> ConfirmPayment([FromBody] PaymentConfirmationDto dto)
//        {
//            await _bookingService.ConfirmBookingPaymentAsync(dto.PaymentReference);
//            return Ok(new { message = "Payment confirmed" });
//        }
//    }
//}

using Microsoft.AspNetCore.Mvc;
using PlazaCore.ServiceContract;
using Shared.DTO.Booking;
using Shared.Security;

namespace PlazaAPI.Controllers
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

        // ------------------------------------------------------
        // GET: api/booking/availability
        // ------------------------------------------------------
        [HttpPost("availability")]
        public async Task<IActionResult> CheckAvailability([FromBody] BookingRequestDto request)
        {
            try
            {
                var isAvailable = await _bookingService.CheckAvailabilityAsync(request);

                return Ok(new
                {
                    success = true,
                    data = new { isAvailable }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // ------------------------------------------------------
        // POST: api/booking/calculate-price
        // ------------------------------------------------------
        [HttpPost("calculate-price")]
        public async Task<IActionResult> CalculatePrice([FromBody] BookingRequestDto request)
        {
            try
            {
                var price = await _bookingService.CalculateTotalPriceAsync(request);

                return Ok(new
                {
                    success = true,
                    data = new { totalPrice = price }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // ------------------------------------------------------
        // POST: api/booking/create
        // ------------------------------------------------------
        [HttpPost("create")]
        public async Task<IActionResult> CreateBooking([FromBody] BookingRequestDto request)
        {
            try
            {
                var booking = await _bookingService.CreateBookingAsync(request);

                return Ok(new
                {
                    success = true,
                    data = booking
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }


        // ------------------------------------------------------
        // POST: api/booking/confirm-payment
        // PayMob → Webhook will use this endpoint
        // ------------------------------------------------------
        [HttpPost("confirm-payment")]
        public async Task<IActionResult> ConfirmPayment([FromQuery] string paymentRef)
        {
            if (string.IsNullOrEmpty(paymentRef))
                return BadRequest(new { success = false, message = "Payment reference missing" });

            try
            {
                await _bookingService.ConfirmBookingPaymentAsync(paymentRef);

                return Ok(new
                {
                    success = true,
                    message = "Booking confirmed successfully"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        // ------------------------------------------------------
        // GET: api/booking/{encodedId}
        // ------------------------------------------------------
        [HttpGet("{encodedId}")]
        public async Task<IActionResult> GetBooking(string encodedId)
        {
            try
            {
                int bookingId = IdEncoder.DecodeId(encodedId);

                var booking = await _bookingService.GetBookingByIdAsync(bookingId);

                if (booking == null)
                    return NotFound(new { success = false, message = "Booking not found" });

                return Ok(new
                {
                    success = true,
                    data = booking
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }
    }
}
