using Microsoft.AspNetCore.Mvc;
using PlazaCore.ServiceContract;
using Shared.DTO.Booking;

namespace PlazaAPI.Controllers
{
    [ApiController]
    [Route("api/hotels")]
    public class RoomAvailabilityController : ControllerBase
    {
        private readonly IRoomAvailabilityService _availabilityService;

        public RoomAvailabilityController(
            IRoomAvailabilityService availabilityService)
        {
            _availabilityService = availabilityService;
        }

        // ------------------------------------------------------
        // POST: api/hotels/{hotelId}/rooms
        // Get all rooms + availability flag
        // ------------------------------------------------------
        [HttpPost("{encodedHotelId}/rooms")]
        public async Task<IActionResult> GetHotelRoomsAvailability(
            string encodedHotelId,
            [FromBody] RoomAvailabilityRequestDto request)
        {
            var result =
                await _availabilityService
                    .GetHotelRoomsAvailabilityAsync(encodedHotelId, request);

            return Ok(new
            {
                success = true,
                data = result
            });
        }

        // ------------------------------------------------------
        // POST: api/hotels/check-availability
        // Check availability for a specific room
        // ------------------------------------------------------
        //[HttpPost("check-availability")]
        //public async Task<IActionResult> CheckAvailability(
        //    [FromBody] BookingRequestDto request)
        //{
        //    bool available =
        //        await _availabilityService.CheckAvailabilityAsync(request);

        //    return Ok(new
        //    {
        //        success = true,
        //        data = new { isAvailable = available }
        //    });
        //}

        // ------------------------------------------------------
        // POST: api/hotels/calculate-price
        // ------------------------------------------------------
        [HttpPost("calculate-price")]
        public async Task<IActionResult> CalculatePrice(
            [FromBody] BookingRequestDto request)
        {
            decimal price =
                await _availabilityService.CalculateTotalPriceAsync(request);

            return Ok(new
            {
                success = true,
                data = new { totalPrice = price }
            });
        }
    }
}
