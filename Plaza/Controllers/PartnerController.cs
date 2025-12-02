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




        //[HttpPost("broadcastEmail/hotel/{encodedId}")]
        //public async Task<IActionResult> StopSales(string encodedId, [FromForm] SendEmailDto dto)
        //{
        //    int hotelId;
        //    try
        //    {
        //        hotelId = IdEncoder.DecodeId(encodedId);
        //    }
        //    catch
        //    {
        //        return BadRequest("Invalid hotel ID");
        //    }
        //    var hotel = await _hotelService.GetHotelAsync(hotelId);
        //    if (hotel == null) return BadRequest("hotelId Not Found");
        //    var hotelDto = _mapper.Map<HotelDto>(hotel);
        //    string? logoUrl = hotelDto.Logo != null ? hotelDto.Logo.Url : hotelDto.Images.FirstOrDefault()?.Url;


        //    var partners = await _partnerService.GetPartnersByHotelAsync(hotelId);

        //    if (!partners.Any())
        //        return Ok(new { message = "No partners found for this hotel" });

        //    var signatureHtml = EmailSign.BuildHotelSignatureHtml(hotelDto, logoUrl);


        //    dto.Body = dto.Body + signatureHtml;

        //    //foreach (var partner in partners)
        //    //{
        //    //    if (!string.IsNullOrEmpty(partner.Email))
        //    //    {
        //    //        await _partnerService.SendEmailToPartner(partner.Id, dto);
        //    //    }
        //    //}
        //    var emailTasks = new List<Task>();
        //    var semaphore = new SemaphoreSlim(5); // نسمح بـ 5 إيميلات تتبعت في نفس اللحظة

        //    foreach (var partner in partners)
        //    {
        //        if (!string.IsNullOrEmpty(partner.Email))
        //        {
        //            await semaphore.WaitAsync();

        //            emailTasks.Add(Task.Run(async () =>
        //            {
        //                try
        //                {
        //                    await _partnerService.SendEmailToPartner(partner.Id, dto);
        //                }
        //                finally
        //                {
        //                    semaphore.Release();
        //                }
        //            }));
        //        }
        //    }

        //    await Task.WhenAll(emailTasks);


        //    return Ok(new { message = "Emails sent to all partners successfully" });
        //}

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

            // هنا بعمل النسخة الداخلية
            var internalDto = new SendEmailInternal
            {
                Subject = dto.Subject,
                Body = dto.Body + signatureHtml,
                AttachStreams = new List<(MemoryStream Stream, string FileName)>()
            };

            // قراءة الملفات مرة واحدة فقط
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

            var emailTasks = new List<Task>();
            var semaphore = new SemaphoreSlim(5);

            foreach (var partner in partners)
            {
                if (string.IsNullOrEmpty(partner.Email))
                    continue;

                await semaphore.WaitAsync();

                emailTasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        await _partnerService.SendEmailToPartner(partner.Id, internalDto);
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                }));
            }

            await Task.WhenAll(emailTasks);

            foreach (var att in internalDto.AttachStreams)
                att.Stream.Dispose();

            return Ok(new { message = "Emails sent to all partners successfully" });
        }


    }

}
