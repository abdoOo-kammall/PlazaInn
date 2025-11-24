using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PlazaCore.Entites;
using PlazaCore.ServiceContract;
using Shared.DTO.Email;
using Shared.DTO.Partner;
using Shared.Security;

namespace Plaza.Controllers
{
    //[Authorize(Roles = "Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class PartnerController : ControllerBase
    {
        private readonly IPartnerService _partnerService;
        private readonly IMapper _mapper;

        public PartnerController(IPartnerService partnerService, IMapper mapper)
        {
            _partnerService = partnerService;
            _mapper = mapper;
        }

        // GET ALL
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PartnerDto>>> GetAll()
        {
            var partners = await _partnerService.GetAllPartnersAsync();
            return Ok(_mapper.Map<IEnumerable<PartnerDto>>(partners));
        }

        // GET BY ID
        [HttpGet("{encodedId}")]
        public async Task<ActionResult<PartnerDto>> Get(string encodedId)
        {
            int id;
            try
            {
                id = IdEncoder.DecodeId(encodedId);
            }
            catch
            {
                return BadRequest("Invalid hotel ID");
            }

            var partner = await _partnerService.GetPartnerAsync(id);

            if (partner == null)
                return Ok(new PartnerDto()); 

            return Ok(_mapper.Map<PartnerDto>(partner));
        }

        // GET BY HOTEL
        [HttpGet("hotel/{encodedId}")]
        public async Task<ActionResult<IEnumerable<PartnerOnlyDto>>> GetByHotel(string encodedId)
        {


            int id;
            try
            {
                id = IdEncoder.DecodeId(encodedId);
            }
            catch
            {
                return BadRequest("Invalid hotel ID");
            }

            var partners = await _partnerService.GetPartnersByHotelAsync(id);

            if (!partners.Any())
                return Ok(new List<PartnerDto>());

            return Ok(_mapper.Map<IEnumerable<PartnerOnlyDto>>(partners));
        }

        // ADD
        [HttpPost]
        public async Task<ActionResult> Add(CreatePartnerDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest();
            var hotelIds = dto.HotelsIds.Select(id => IdEncoder.DecodeId(id)).ToList();


            var partner = _mapper.Map<Partners>(dto);
            partner.Hotels = hotelIds.Select(id => new Hotel { Id = id }).ToList();

            await _partnerService.AddPartnerAsync(partner);

            return Ok();
        }

        // UPDATE
        [HttpPut]
        public async Task<ActionResult> Update( string encodedId,UpdatePartnerDto dto)
        {
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
                return BadRequest();
            var partnerExist = await _partnerService.GetPartnerAsync(id);
            if (partnerExist == null) return Ok();
            var hotelIds = dto.HotelsIds.Select(id => IdEncoder.DecodeId(id)).ToList();

            var partner = _mapper.Map(dto,partnerExist);
            partner.Hotels = hotelIds.Select(id => new Hotel { Id = id }).ToList();

            await _partnerService.UpdatePartnerAsync(partner);

            return NoContent();
        }

        // DELETE
        [HttpDelete("{encodedId}")]
        public async Task<ActionResult> Delete(string encodedId)

        {
            int id;
            try
            {
                id = IdEncoder.DecodeId(encodedId);
            }
            catch
            {
                return BadRequest("Invalid hotel ID");
            }
            var partnerExist = await _partnerService.GetPartnerAsync(id);
            if (partnerExist == null) return Ok();
            await _partnerService.DeletePartnerAsync(id);
            return NoContent();
        }

        [HttpPost("{partnerId}/send-email")]
        public async Task<IActionResult> SendEmailToPartner(string partnerId, SendEmailDto dto)
        {
            int id;
            try
            {
                id = IdEncoder.DecodeId(partnerId);
            }
            catch
            {
                return BadRequest("Invalid hotel ID");
            }
            await _partnerService.SendEmailToPartner(id, dto);
            return Ok(new { message = "Email sent successfully" });
        }

    }

}
