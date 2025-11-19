using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic.FileIO;
using PlazaCore.ServiceContract;
using Shared.Enums;

namespace Plaza.Controllers
{
    //[Authorize(Roles = "Admin")]

    [Route("api/")]
    [ApiController]
    public class FileController : ControllerBase
    {
        private readonly IFileService _imageService;
        private readonly IWebHostEnvironment _env;

        public FileController(IFileService imageService, IWebHostEnvironment env)
        {
            _imageService = imageService;
            _env = env;
        }
        #region old One 
        //[HttpPost]
        //public async Task<IActionResult> UploadImages([FromForm] List<IFormFile> files, [FromQuery] PlazaInnType? entityType)
        //{
        //    if (entityType == null || !Enum.IsDefined(typeof(PlazaInnType), entityType))
        //        return BadRequest("Invalid Image");
        //    if (files == null || files.Count == 0)
        //        return BadRequest("No files uploaded.");

        //    try
        //    {
        //        var ids = await _imageService.SaveImagesAsync(files, entityType);
        //        return Ok(ids);
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, $"🔥 Internal Error: {ex.Message} | {ex.InnerException?.Message}");
        //    }
        //}
        #endregion
        [HttpPost("upload")]
        public async Task<IActionResult> UploadFiles(
            [FromForm] List<IFormFile> files,
            [FromQuery] PlazaInnType type)
        {
            if (files == null || files.Count == 0)
                return BadRequest("No files uploaded.");

            PlazaInnType fileType;

            if (!Enum.TryParse(type.ToString(), true, out fileType))
                return BadRequest("Invalid file type.");

            try
            {
                var ids = await _imageService.SaveFilesAsync(files, fileType);
                return Ok(ids ); 
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"🔥 Internal Error: {ex.Message} | {ex.InnerException?.Message}");
            }
        }













    }
}
