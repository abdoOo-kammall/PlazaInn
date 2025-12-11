using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PlazaCore.ServiceContract;
using Shared.DTO.AboutUs;

namespace Plaza.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AboutUsController : ControllerBase
    {
        private readonly IAboutUsService _aboutUsService;

        public AboutUsController(IAboutUsService aboutUsService)
        {
            this._aboutUsService = aboutUsService;
        }
        [HttpGet]
        public async Task<ActionResult> GetAllAboutUsImages()
        {
            var aboutUsImages = await _aboutUsService.getAllAboutUsImages();
            return Ok(aboutUsImages);
        }
        [HttpGet("{id}")]
        public async Task<ActionResult> GetAboutUsImage(int id)
        {
            var aboutUsImage = await _aboutUsService.getAboutUsImagesById(id);
            if (aboutUsImage == null) return Ok();
            return Ok(aboutUsImage);
        }
        [HttpPost]
        public async Task<ActionResult> addAboutUsImages(CreateAboutUsDto aboutUsDto)
        {

            await _aboutUsService.addAboutUsImages(aboutUsDto);
            return Ok("Added Succuesfully");
        }
        [HttpPut("{id}")]
        public async Task<ActionResult> updateAboutUsImages(int id, UpdateAboutUsDto aboutUsDto)
        {

            await _aboutUsService.updateAboutUsImages(id, aboutUsDto);
            return NoContent();
        }
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteAboutUsImages(int id)
        {

            await _aboutUsService.deleteAboutUsImages(id);
            return NoContent();
        }
    }
}
