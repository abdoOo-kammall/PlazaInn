using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PlazaCore.ServiceContract;
using Shared.DTO.Payment;

namespace Plaza.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymobService _paymob;

        public PaymentController(IPaymobService paymobService)
        {
            this._paymob = paymobService;
        }
        [HttpPost("test")]
        public async Task<ActionResult<string>> testPayment(PaymentRequestDto requestDto)
        {

            var url = await _paymob.CreatePaymentLinkAsync(requestDto, false);
            return Ok(new { paymentUrl = url });
        
        }
    }
}
