using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PlazaCore.ServiceContract;
using Shared.DTO.HomeSlider_AboutUs;
using Shared.Enums;

namespace Plaza.Controllers
{
    [Route("api/section-images")]
    [ApiController]
    public class SectionImagesController : ControllerBase
    {
        private readonly ISectionImageService _service;

        public SectionImagesController(ISectionImageService service)
        {
            _service = service;
        }

        // GET ?type=AboutUs
        [HttpGet]
        public async Task<IActionResult> GetImages([FromQuery] PlazaInnType type)
        {
            var images = await _service.GetImagesByTypeAsync(type);
            return Ok(images);
        }

        // PUT ?type=AboutUs
        [HttpPut]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateImages([FromQuery] PlazaInnType type, UpdateSectionImagesDto dto)
        {
            await _service.UpdateSectionImagesAsync(type, dto.ImagesId);
            return NoContent();
        }
    }

}
