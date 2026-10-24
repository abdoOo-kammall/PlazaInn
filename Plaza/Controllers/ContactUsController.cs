using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlazaCore.ServiceContract;
using Shared.DTO.ContactUs;

namespace PlazaApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContactUsController : ControllerBase
    {
        private readonly IContactUsService _contactUsService;

        public ContactUsController(IContactUsService contactUsService)
        {
            _contactUsService = contactUsService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ContactUsDto>>> GetAll()
        {
            var result = await _contactUsService.GetAllContactUs();

            return Ok(result ?? new ContactUsDto());
        }



        [HttpPut]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update( [FromBody] UpdateContactUsDto dto)
        {
            
            await _contactUsService.UpdateContactUs( dto);
            return NoContent();
        }

       
    }
}
