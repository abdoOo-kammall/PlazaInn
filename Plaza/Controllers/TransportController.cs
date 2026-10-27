using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Plaza.Extensions;
using PlazaCore.Entites;
using PlazaCore.ServiceContract;
using Shared.DTO.Transport;
using Shared.Security;

namespace Plaza.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class TransportController : ControllerBase
    {
        private readonly ITransportService _transportService;
        private readonly IMapper _mapper;

        public TransportController(ITransportService transportService, IMapper mapper)
        {
            _transportService = transportService;
            _mapper = mapper;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<TransportDto>>> GetTransports([FromQuery] TransportFilterDto filter)
        {
            var transports = await _transportService.GetTransportsAsync(filter, activeOnly: !User.IsAdmin());
            return Ok(_mapper.Map<IEnumerable<TransportDto>>(transports));
        }

        [HttpGet("lookup")]
        [AllowAnonymous]
        public async Task<IActionResult> GetLookup()
        {
            var transports = await _transportService.GetTransportsAsync(new TransportFilterDto(), activeOnly: !User.IsAdmin());
            var result = transports.Select(transport => new
            {
                Id = IdEncoder.EncodeId(transport.Id),
                Name = transport.Name,
                NameAr = transport.NameAr,
                VehicleType = transport.VehicleType
            });
            return Ok(result);
        }

        [HttpGet("{encodedId}")]
        [AllowAnonymous]
        public async Task<ActionResult<TransportDto>> GetTransport(string encodedId)
        {
            if (!IdEncoding.TryDecode(encodedId, out var id))
                return BadRequest("Invalid transport ID");

            var transport = await _transportService.GetTransportAsync(id);
            if (transport == null || (!transport.IsActive && !User.IsAdmin()))
                return NotFound();

            return Ok(_mapper.Map<TransportDto>(transport));
        }

        [HttpPost]
        public async Task<ActionResult> AddTransport([FromBody] CreateTransportDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var transport = _mapper.Map<Transport>(dto);
            await _transportService.AddTransportAsync(transport);
            var created = _mapper.Map<TransportDto>(transport);
            return CreatedAtAction(nameof(GetTransport), new { encodedId = IdEncoder.EncodeId(transport.Id) }, created);
        }

        [HttpPut("{encodedId}")]
        public async Task<ActionResult> UpdateTransport(string encodedId, [FromBody] UpdateTransportDto dto)
        {
            if (!IdEncoding.TryDecode(encodedId, out var id))
                return BadRequest("Invalid transport ID");
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existing = await _transportService.GetTransportAsync(id);
            if (existing == null)
                return NotFound();

            _mapper.Map(dto, existing);
            await _transportService.UpdateTransportAsync(existing);
            return NoContent();
        }

        [HttpDelete("{encodedId}")]
        public async Task<ActionResult> DeleteTransport(string encodedId)
        {
            if (!IdEncoding.TryDecode(encodedId, out var id))
                return BadRequest("Invalid transport ID");

            var transport = await _transportService.GetTransportAsync(id);
            if (transport == null)
                return NotFound();

            await _transportService.DeleteTransportAsync(id);
            return NoContent();
        }
    }
}
