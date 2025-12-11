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
        public async Task<IActionResult> GetAll()
        {
            var result = await _contactUsService.GetAllContactUs();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var result = await _contactUsService.GetContactUs(id);
            if (result == null)
                return NotFound();

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateContactUsDto dto)
        {
            await _contactUsService.CreateContactUs(dto);
            return Ok(new { message = "Created Successfully" });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateContactUsDto dto)
        {
            await _contactUsService.UpdateContactUs(id, dto);
            return Ok(new { message = "Updated Successfully" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _contactUsService.DeleteContactUs(id);
            return Ok(new { message = "Deleted Successfully" });
        }
    }
}
