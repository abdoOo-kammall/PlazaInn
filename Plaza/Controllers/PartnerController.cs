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
    [Authorize(Roles = "Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class PartnerController : ControllerBase
    {
        private readonly IPartnerService _partnerService;
        private readonly IHotelService _hotelService;
        private readonly IMapper _mapper;
        private readonly ILogger<PartnerController> _logger;

        public PartnerController(IPartnerService partnerService, IHotelService hotelService , IMapper mapper, ILogger<PartnerController> logger)
        {
            _partnerService = partnerService;
            this._hotelService = hotelService;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PartnerDto>>> GetAll()
        {
            var partners = await _partnerService.GetAllPartnersAsync();
            return Ok(_mapper.Map<IEnumerable<PartnerDto>>(partners));
        }

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
                return NotFound(); 

            return Ok(_mapper.Map<PartnerDto>(partner));
        }

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

        [HttpPost]
        public async Task<ActionResult> Add(CreatePartnerDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest();
            var hotelIds = dto.HotelsIds.Select(id => IdEncoder.DecodeId(id)).ToList();


            var partner = _mapper.Map<Partners>(dto);
            partner.Hotels = hotelIds.Select(id => new Hotel { Id = id }).ToList();

            await _partnerService.AddPartnerAsync(partner);

            return Created();
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

            var hotelIds = dto.HotelsIds.Select(hId => IdEncoder.DecodeId(hId)).Distinct().ToList();
            var hotels = await _hotelService.GetHotelsByIdsAsync(hotelIds);
            partnerExist.Hotels = hotels.ToList();

            _mapper.Map(dto, partnerExist);

            await _partnerService.UpdatePartnerAsync(partnerExist);

            return NoContent();
        }


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
            if (partnerExist == null) return NotFound();
            await _partnerService.DeletePartnerAsync(id);
            return NoContent();
        }



    
        [HttpPost("broadcastEmail/hotel/{encodedId}")]
        public async Task<IActionResult> StopSales(string encodedId, [FromForm] SendEmailRequestDto dto)
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
            if (hotel == null)
                return BadRequest("hotelId Not Found");

            var hotelDto = _mapper.Map<HotelDto>(hotel);
            string? logoUrl = hotelDto.Logo?.Url ?? hotelDto.Images.FirstOrDefault()?.Url;

            var partners = await _partnerService.GetPartnersByHotelAsync(hotelId);
            if (!partners.Any())
                return Ok(new { message = "No partners found for this hotel" });

            var signatureHtml = EmailSign.BuildHotelSignatureHtml(hotelDto, logoUrl);

            var internalDto = new SendEmailInternal
            {
                Subject = dto.Subject,
                Body = dto.Body + signatureHtml,
                AttachStreams = new List<(MemoryStream Stream, string FileName)>()
            };

            try
            {
                if (dto.Attachments != null)
                {
                    foreach (var file in dto.Attachments)
                    {
                        var ms = new MemoryStream();
                        await file.CopyToAsync(ms);
                        ms.Position = 0;
                        internalDto.AttachStreams.Add((ms, file.FileName));
                    }
                }

                // Emails are sent with already-loaded partner data only; the scoped DbContext must not be touched in parallel.
                var recipients = partners
                    .Where(p => !string.IsNullOrWhiteSpace(p.Email))
                    .Select(p => p.Email)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                using var semaphore = new SemaphoreSlim(5);
                var failed = 0;

                var emailTasks = recipients.Select(async email =>
                {
                    await semaphore.WaitAsync();
                    try
                    {
                        await _partnerService.SendEmailToPartner(email, internalDto);
                    }
                    catch (Exception ex)
                    {
                        Interlocked.Increment(ref failed);
                        _logger.LogError(ex, "Failed to send broadcast email for hotel {HotelId} to {Email}.", hotelId, email);
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                });

                await Task.WhenAll(emailTasks);

                var sent = recipients.Count - failed;
                if (failed > 0)
                    return StatusCode(StatusCodes.Status207MultiStatus, new { message = $"Emails sent to {sent} partner(s); {failed} failed.", sent, failed });

                return Ok(new { message = "Emails sent to all partners successfully", sent, failed });
            }
            finally
            {
                foreach (var att in internalDto.AttachStreams)
                    att.Stream.Dispose();
            }
        }


    }

}
