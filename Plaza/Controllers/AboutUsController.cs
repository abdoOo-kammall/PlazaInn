using Microsoft.AspNetCore.Authorization;
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
      
        //[HttpPost]
        //public async Task<ActionResult> addAboutUsImages(CreateAboutUsDto aboutUsDto)
        //{

        //    await _aboutUsService.addAboutUsImages(aboutUsDto);
        //    return Ok("Added Succuesfully");
        //}
        [HttpPut]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> updateAboutUsImages( UpdateAboutUsDto aboutUsDto)
        {

            await _aboutUsService.updateAboutUsImages(aboutUsDto);
            return NoContent();
        }
      
    }
}
