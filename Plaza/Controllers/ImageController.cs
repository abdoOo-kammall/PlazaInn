using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlazaCore.ServiceContract;
using Shared.Enums;

namespace Plaza.Controllers
{
    [Authorize(Roles = "Admin")]

    [Route("api/[controller]")]
    [ApiController]
    public class ImageController : ControllerBase
    {
        private readonly IImageService _imageService;
        private readonly IWebHostEnvironment _env;

        public ImageController(IImageService imageService, IWebHostEnvironment env)
        {
            _imageService = imageService;
            _env = env;
        }

        [HttpPost]
        public async Task<IActionResult> UploadImages([FromForm] List<IFormFile> files, [FromQuery] PlazaInnType? entityType)
        {
            if (entityType == null || !Enum.IsDefined(typeof(PlazaInnType), entityType))
                return BadRequest("Invalid Image");
            if (files == null || files.Count == 0)
                return BadRequest("No files uploaded.");

            try
            {
                var ids = await _imageService.SaveImagesAsync(files, entityType);
                return Ok(ids);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"🔥 Internal Error: {ex.Message} | {ex.InnerException?.Message}");
            }
        }













      
    }
}
