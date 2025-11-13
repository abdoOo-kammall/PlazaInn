using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PlazaCore.ServiceContract;
using Shared.DTO.Partner;

namespace Plaza.Controllers
{
    [Authorize(Roles = "Admin")]

    [Route("api/[controller]")]
    [ApiController]
    public class PartnerController : ControllerBase
    {
        private readonly IPartnerService _partnerService;

        public PartnerController(IPartnerService partnerService)
        {
            this._partnerService = partnerService;
        }
        [HttpGet]

        public async Task<ActionResult<PartnerDto>> getAllPartners() { 
        
             var partners = await _partnerService.GetAllPartnersAsync();
            return Ok(partners);
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<PartnerDto>> getPartner(int id) { 
            var partner = await _partnerService.GetPartnerAsync(id); 
            if (partner == null) { return NotFound(); }
            return Ok(partner);
        }
        //[HttpPost]
        //public Task<ActionResult> addPartner(PartnerDto partner) {
        //    if (!ModelState.IsValid) { return BadRequest(); }
        //}
    }
}
