using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PlazaCore.ServiceContract;
using Shared.DTO.InsightHotel;
using Shared.Security;
using PlazaCore.Entites;
using Microsoft.AspNetCore.Authorization;

namespace Plaza.Controllers
{
    [Authorize(Roles = "Admin")]

    [Route("api/[controller]")]
    [ApiController]
    public class InsightHotelController : ControllerBase
    {
        private readonly IInsightHotelService _insightService;
        private readonly IHotelService _hotelService;
        private readonly IMapper _mapper;

        public InsightHotelController(
            IInsightHotelService insightService,
            IHotelService hotelService,
            IMapper mapper)
        {
            _insightService = insightService;
            _hotelService = hotelService;
            _mapper = mapper;
        }

        [HttpPost]



        public async Task<ActionResult> AddInsightHotel([FromBody] CreateInsightHotelDto dto)
        {
            // فك الـ EncodedHotelId
            int hotelId;
            try
            {
                hotelId = IdEncoder.DecodeId(dto.EncodedHotelId);
            }
            catch
            {
                return BadRequest("Invalid hotel ID");
            }

            var hotel = await _hotelService.GetHotelAsync(hotelId);
            if (hotel == null)
                return NotFound("Hotel not found");

            var insight = _mapper.Map<InsightHotel>(dto);

            insight.HotelId = hotelId;
            try
            {
                await _insightService.AddInsightHotelAsync(insight);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = ex.Message,
                    inner = ex.InnerException?.Message
                });
            }


            //await _insightService.AddInsightHotelAsync(insight);

            return CreatedAtAction(nameof(GetInsightHotel),
                                   new { encodedHotelId = dto.EncodedHotelId },
                                   dto);
        }

        [HttpGet("{encodedHotelId}")]
        public async Task<ActionResult<InsightHotelDto>> GetInsightHotel(string encodedHotelId)
        {
            int hotelId;
            try
            {
                hotelId = IdEncoder.DecodeId(encodedHotelId);
            }
            catch
            {
                return BadRequest("Invalid hotel ID");
            }

            var insight = await _insightService.GetByHotelIdAsync(hotelId);
            if (insight == null)
                return NotFound();

            var result = _mapper.Map<InsightHotelDto>(insight);

            return Ok(result);
        }

        [HttpPut("{encodedHotelId}")]
        public async Task<ActionResult> UpdateInsightHotel(string encodedHotelId, [FromBody] InsightHotelDto dto)
        {
            int hotelId;
            try
            {
                hotelId = IdEncoder.DecodeId(encodedHotelId);
            }
            catch
            {
                return BadRequest("Invalid hotel ID");
            }

            var existing = await _insightService.GetByHotelIdAsync(hotelId);
            if (existing == null)
                return NotFound();

            
            _mapper.Map(dto, existing);

            await _insightService.UpdateInsightHotelAsync(existing);

            return NoContent();
        }

        [HttpDelete("{encodedHotelId}")]
        public async Task<ActionResult> DeleteInsightHotel(string encodedHotelId)
        {
            int hotelId;
            try
            {
                hotelId = IdEncoder.DecodeId(encodedHotelId);
            }
            catch
            {
                return BadRequest("Invalid hotel ID");
            }

            var insight = await _insightService.GetByHotelIdAsync(hotelId);
            if (insight == null)
                return NotFound();

            await _insightService.DeleteInsightHotelAsync(hotelId);

            return NoContent();
        }
    }
}
