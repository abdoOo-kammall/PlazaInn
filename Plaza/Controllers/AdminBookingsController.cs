using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlazaCore.ServiceContract;
using Shared.Enums;
using System;
using System.Threading.Tasks;

namespace Plaza.Controllers
{
    [ApiController]
    [Route("api/admin/bookings")]
    [Authorize(Roles = "Admin")]
    public class AdminBookingsController : ControllerBase
    {
        private readonly IBookingService _bookingService;

        public AdminBookingsController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int? hotelId,
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            [FromQuery] BookingStatus? status)
        {
            var bookings = await _bookingService.GetAdminBookingsAsync(hotelId, from, to, status);
            return Ok(new { success = true, data = bookings });
        }
    }
}

