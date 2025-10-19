using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PlazaCore.ServiceContract;
using PlazaService.Hotels;

namespace Plaza.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImageController : ControllerBase
    {
        private readonly IImageService _imageService;

        public ImageController(IImageService imageService)
        {
            this._imageService = imageService;
        }

        [HttpPost]
        public async Task<ActionResult<List<int>>> UploadImages([FromForm] List<IFormFile> files, string? entityType)
        {

            if (files == null || files.Count == 0)
                return BadRequest("No files uploaded.");
            var ids = await _imageService.SaveImagesAsync(files, entityType);
            return Ok(ids);
        }
    }
}
