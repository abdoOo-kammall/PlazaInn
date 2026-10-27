using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Plaza.Extensions;
using PlazaCore.ServiceContract;
using Shared.DTO.Booking;
using Shared.Enums;
using Shared.Security;
using System;

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
        [Authorize]
        [EnableRateLimiting(RateLimitingExtensions.BookingCreationPolicy)]
        public async Task<IActionResult> CreateBooking([FromBody] BookingRequestDto request)
        {
            var userId = User.GetUserId();
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized(new { success = false, message = "User id claim is missing in token." });

            var booking = await _bookingService.CreateBookingAsync(request, userId, User.GetEmail());
            return Ok(new { success = true, data = booking });
        }

        [HttpGet("my")]
        [Authorize]
        public async Task<IActionResult> GetMyBookings(
            [FromQuery] int? hotelId,
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            [FromQuery] BookingStatus? status,
            [FromQuery] BookingType? bookingType)
        {
            var userId = User.GetUserId();
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized(new { success = false, message = "User id claim is missing in token." });

            var bookings = await _bookingService.GetUserBookingsAsync(userId, hotelId, from, to, status, bookingType);
            return Ok(new { success = true, data = bookings });
        }

        [HttpGet("{encodedId}")]
        [Authorize]
        public async Task<IActionResult> GetBookingById(string encodedId)
        {
            var userId = User.GetUserId();
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized(new { success = false, message = "User id claim is missing in token." });

            var booking = await _bookingService.GetBookingByIdAsync(
                IdEncoder.DecodeId(encodedId), userId, User.IsAdmin());

            return Ok(new { success = true, data = booking });
        }

        [HttpPost("{encodedId}/cancel")]
        [Authorize]
        public async Task<IActionResult> CancelBooking(string encodedId)
        {
            var userId = User.GetUserId();
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized(new { success = false, message = "User id claim is missing in token." });

            var booking = await _bookingService.CancelBookingAsync(
                IdEncoder.DecodeId(encodedId), userId, User.IsAdmin());

            return Ok(new { success = true, data = booking });
        }
    }

}
