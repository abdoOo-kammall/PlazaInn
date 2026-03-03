using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PlazaCore.ServiceContract;
using Shared.DTO.Payment;

namespace Plaza.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WebhookController : ControllerBase
    {
        private readonly IBookingService _bookingService;

        public WebhookController( IBookingService bookingService)
        {
            this._bookingService = bookingService;
        }
        [HttpPost("callback")]
        public async Task<IActionResult> PaymobCallback([FromBody] PaymobCallbackDto dto)
        {
            if (!dto.success) return Ok();

            await _bookingService.ConfirmBookingPaymentAsync(
                dto.order.id.ToString()
            );

            return Ok();
        }

    }
}
