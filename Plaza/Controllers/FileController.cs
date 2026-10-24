using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlazaCore.ServiceContract;
using Shared.Enums;

namespace Plaza.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("api/")]
    [ApiController]
    public class FileController : ControllerBase
    {
        private const long MaxRequestBytes = 50L * 1024 * 1024;

        private readonly IFileService _imageService;

        public FileController(IFileService imageService)
        {
            _imageService = imageService;
        }

        [HttpPost("upload")]
        [RequestSizeLimit(MaxRequestBytes)]
        [RequestFormLimits(MultipartBodyLengthLimit = MaxRequestBytes)]
        public async Task<IActionResult> UploadFiles(
            [FromForm] List<IFormFile> files,
            [FromQuery] PlazaInnType type)
        {
            if (files == null || files.Count == 0)
                return BadRequest("No files uploaded.");

            if (!Enum.IsDefined(typeof(PlazaInnType), type) || type == PlazaInnType.None)
                return BadRequest("Invalid file type.");

            try
            {
                var ids = await _imageService.SaveFilesAsync(files, type);
                return Ok(ids);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
