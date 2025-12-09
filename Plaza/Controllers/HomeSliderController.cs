using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using PlazaCore.ServiceContract;
using Shared.DTO.HomeSlider;
using Shared.Security;

namespace Plaza.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HomeSliderController : ControllerBase
    {
        private readonly IHomeSliderService _homeSliderService;

        public HomeSliderController(IHomeSliderService homeSliderService)
        {
            this._homeSliderService = homeSliderService;
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<HomeSliderDto>>> getAllHomeSlider()
        {


            var homeSliders = await _homeSliderService.GetAllHomeSliderAsync();
            //var homeSlidersArranged = homeSliders.SelectMany(src => src.Images).ToList();
            return Ok(homeSliders);
        }
        [HttpPost]
        public async Task<ActionResult> addHomeSlider(CreateHomeSliderDto homeSliderDto)
        {

            await _homeSliderService.AddHomeSliderAsync(homeSliderDto);
            return Ok("HomeSliders have been added Succesfully");
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<HomeSliderDto>> getHomeSlider(int id)
        {


            return Ok(await _homeSliderService.GetHomeSliderAsync(id));
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> updateHomeSlider(int id, UpdateHomeSliderDto homeSliderDto)
        {


            await _homeSliderService.UpdateHomeSliderAsync(id, homeSliderDto);
            return NoContent();

        }
        [HttpDelete("{id}")]
        public async Task<ActionResult> deleteHomeSlider(int id)
        {

            await _homeSliderService.DeleteHomeSliderAsync(id);
            return NoContent();
        }
    }
}
