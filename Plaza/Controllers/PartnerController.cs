using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PlazaCore.Entites;
using PlazaCore.ServiceContract;
using PlazaCore.ServiceContract.Account;
using Shared.DTO.Email;
using Shared.DTO.Hotel;
using Shared.DTO.Partner;
using Shared.EmailSignture;
using Shared.Security;

namespace Plaza.Controllers
{
    //[Authorize(Roles = "Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class PartnerController : ControllerBase
    {
        private readonly IPartnerService _partnerService;
        private readonly IHotelService _hotelService;
        private readonly IMapper _mapper;

        public PartnerController(IPartnerService partnerService, IHotelService hotelService , IMapper mapper)
        {
            _partnerService = partnerService;
            this._hotelService = hotelService;
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

        [HttpPut("{encodedId}")]
        public async Task<ActionResult> Update(string encodedId, UpdatePartnerDto dto)
        {
            int id;
            try
            {
                id = IdEncoder.DecodeId(encodedId);
            }
            catch
            {
                return BadRequest("Invalid partner ID");
            }

            if (!ModelState.IsValid)
                return BadRequest();

            var partnerExist = await _partnerService.GetPartnerAsync(id);
            if (partnerExist == null)
                return NotFound();

            // Load الفنادق من DB حسب IDs اللي جاية
            var hotelIds = dto.HotelsIds.Select(hId => IdEncoder.DecodeId(hId)).ToList();
            var hotels = await _hotelService.GetAllHotelsAsync(); // أو طريقة فلترة حسب hotelIds
            partnerExist.Hotels = hotels.Where(h => hotelIds.Contains(h.Id)).ToList();

            // Map باقي البيانات
            _mapper.Map(dto, partnerExist);

            await _partnerService.UpdatePartnerAsync(partnerExist);

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

        //[HttpPost("{partnerId}/send-email")]
        //public async Task<IActionResult> SendEmailToPartner(string partnerId, SendEmailDto dto)
        //{
        //    int id;
        //    try
        //    {
        //        id = IdEncoder.DecodeId(partnerId);
        //    }
        //    catch
        //    {
        //        return BadRequest("Invalid hotel ID");
        //    }
        //    await _partnerService.SendEmailToPartner(id, dto);
        //    return Ok(new { message = "Email sent successfully" });
        //}

        [HttpPost("broadcastEmail/hotel/{encodedId}")]
        public async Task<IActionResult> StopSales(string encodedId, SendEmailDto dto)
        {
            int hotelId;
            try
            {
                hotelId = IdEncoder.DecodeId(encodedId);
            }
            catch
            {
                return BadRequest("Invalid hotel ID");
            }
            var hotel = await _hotelService.GetHotelAsync(hotelId);
            if (hotel == null) return BadRequest("hotelId Not Found");
            var hotelDto = _mapper.Map<HotelDto>(hotel);
            string? logoUrl = hotelDto.Logo != null ? hotelDto.Logo.Url : hotelDto.Images.FirstOrDefault()?.Url;


            var partners = await _partnerService.GetPartnersByHotelAsync(hotelId);

            if (!partners.Any())
                return Ok(new { message = "No partners found for this hotel" });

            var signatureHtml = EmailSign.BuildHotelSignatureHtml(hotelDto, logoUrl);


            dto.Body = dto.Body + signatureHtml;

            foreach (var partner in partners)
            {
                if (!string.IsNullOrEmpty(partner.Email))
                {
                    await _partnerService.SendEmailToPartner(partner.Id, dto);
                }
            }

            return Ok(new { message = "Emails sent to all partners successfully" });
        }


    }

}
