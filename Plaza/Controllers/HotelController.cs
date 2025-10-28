using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PlazaCore.Entites;
using PlazaCore.ServiceContract;
using PlazaService.Hotels;
using Shared.DTO.Hotel;
using Shared.Security;

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
        [HttpGet("{encodedId}")]
        public async Task<ActionResult<HotelDto>> GetHotel(string encodedId) {

            int id;
            try
            {
                id = IdEncoder.DecodeId(encodedId);
            }
            catch
            {
                return BadRequest("Invalid hotel ID");
            }
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

            //await _hotelService.UpdateHotelAsync(hotel);

            var createdDto = _mapper.Map<HotelDto>(hotel);
            var encodedId = IdEncoder.EncodeId(hotel.Id);


            return CreatedAtAction(nameof(GetHotel), new { encodedId }, createdDto);
            
            }
            [HttpPut("{encodedId}")]
            public async Task<ActionResult> UpdateHotel(string encodedId, [FromBody] UpdateHotelDTO hotelDto) {

            //if (id != hotelDto.Id )
            //{
            //    return BadRequest();
            //}
            int id;
            try
            {
                id = IdEncoder.DecodeId(encodedId);
            }
            catch
            {
                return BadRequest("Invalid hotel ID");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var existing = await _hotelService.GetHotelAsync(id);
                if (existing == null)
                    return NotFound();
                _mapper.Map(hotelDto, existing);

            await _hotelService.UpdateHotelAsync(existing);
                return NoContent();
            }

        [HttpDelete("{encodedId}")]
        public async Task<ActionResult> DeleteHotel(string encodedId) {
            int id;
            try
            {
                id = IdEncoder.DecodeId(encodedId);
            }
            catch
            {
                return BadRequest("Invalid hotel ID");
            }
            var hotel = await _hotelService.GetHotelAsync(id);
            if (hotel == null) return NotFound();
            await _hotelService.DeleteHotelAsync(id);
            return NoContent();
        }
    }
}
