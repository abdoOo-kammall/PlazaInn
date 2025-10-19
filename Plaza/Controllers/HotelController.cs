using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PlazaCore.Entites;
using PlazaCore.ServiceContract;
using PlazaService.Hotels;
using Shared.DTO.Hotel;

namespace Plaza.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HotelController : ControllerBase
    {
        private readonly IHotelService _hotelService;
        private readonly IMapper _mapper;

        public HotelController(IHotelService hotelService , IMapper mapper)
        {
            _hotelService = hotelService;
            this._mapper = mapper;
        }
        [HttpGet]
        public async Task <ActionResult<IEnumerable<HotelDto>>> GetHotels() { 
            
            var hotels = await _hotelService.GetAllHotelsAsync();
            var hotelsResult = _mapper.Map<IEnumerable<HotelDto>>(hotels);
            return Ok(hotelsResult);
        }
        [HttpGet("{id:int}")]
        public async Task<ActionResult<HotelDto>> GetHotel(int id) { 
            var hotel = await _hotelService.GetHotelAsync(id);
            if (hotel == null) return NotFound();
            var hotelResult = _mapper.Map<HotelDto>(hotel);

            return Ok(hotelResult);
            
        }
            [HttpPost]
            public async Task<ActionResult> AddHotel([FromBody] CreateHotelDto hotelDto) {

                if (!ModelState.IsValid) { 
                    return BadRequest(ModelState);
                }
                var hotel =  _mapper.Map<Hotel>(hotelDto);
                await _hotelService.AddHotelAsync(hotel);

            await _hotelService.UpdateHotelAsync(hotel);

            var createdDto = _mapper.Map<HotelDto>(hotel);

                return CreatedAtAction(nameof(GetHotel), new { id = createdDto.Id }, createdDto);
            
            }
            [HttpPut("{id:int}")]
            public async Task<ActionResult> UpdateHotel(int id , [FromBody] UpdateHotelDTO hotelDto) {

                //if (id != hotelDto.Id )
                //{
                //    return BadRequest();
                //}
                var existing = await _hotelService.GetHotelAsync(id);
                if (existing == null)
                    return NotFound();
                _mapper.Map(hotelDto, existing);

            await _hotelService.UpdateHotelAsync(existing);
                return NoContent();
            }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> DeleteHotel(int id) {
            var hotel = await _hotelService.GetHotelAsync(id);
            if (hotel == null) return NotFound();
            await _hotelService.DeleteHotelAsync(id);
            return NoContent();
        }
    }
}
