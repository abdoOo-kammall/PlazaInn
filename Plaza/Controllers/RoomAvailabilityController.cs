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

        public RoomAvailabilityController(IRoomAvailabilityService availabilityService)
        {
            _availabilityService = availabilityService;
        }

        [HttpPost("{encodedHotelId}/rooms")]
        public async Task<IActionResult> GetHotelRoomsAvailability(string encodedHotelId, [FromBody] RoomAvailabilityRequestDto request)
        {
            var result = await _availabilityService.GetHotelRoomsAvailabilityAsync(encodedHotelId, request);
            return Ok(new { success = true, data = result });
        }

        [HttpPost("calculate-price")]
        public async Task<IActionResult> CalculatePrice([FromBody] BookingRequestDto request)
        {
            decimal price = await _availabilityService.CalculateTotalPriceAsync(request);
            return Ok(new { success = true, data = new { totalPrice = price } });
        }
    }


}
