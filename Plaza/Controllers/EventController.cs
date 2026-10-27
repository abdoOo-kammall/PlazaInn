using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Plaza.Extensions;
using PlazaCore.Entites;
using PlazaCore.ServiceContract;
using Shared.DTO.Event;
using Shared.Security;

namespace Plaza.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class EventController : ControllerBase
    {
        private readonly IEventService _eventService;
        private readonly IMapper _mapper;

        public EventController(IEventService eventService, IMapper mapper)
        {
            _eventService = eventService;
            _mapper = mapper;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<EventDto>>> GetEvents([FromQuery] EventFilterDto filter)
        {
            var events = await _eventService.GetEventsAsync(filter, activeOnly: !User.IsAdmin());
            return Ok(_mapper.Map<IEnumerable<EventDto>>(events));
        }

        [HttpGet("lookup")]
        [AllowAnonymous]
        public async Task<IActionResult> GetLookup()
        {
            var events = await _eventService.GetEventsAsync(new EventFilterDto(), activeOnly: !User.IsAdmin());
            var result = events.Select(item => new
            {
                Id = IdEncoder.EncodeId(item.Id),
                Name = item.Name,
                NameAr = item.NameAr,
                EventDate = item.EventDate
            });
            return Ok(result);
        }

        [HttpGet("{encodedId}")]
        [AllowAnonymous]
        public async Task<ActionResult<EventDto>> GetEvent(string encodedId)
        {
            if (!IdEncoding.TryDecode(encodedId, out var id))
                return BadRequest("Invalid event ID");

            var entity = await _eventService.GetEventAsync(id);
            if (entity == null || (!entity.IsActive && !User.IsAdmin()))
                return NotFound();

            return Ok(_mapper.Map<EventDto>(entity));
        }

        [HttpPost]
        public async Task<ActionResult> AddEvent([FromBody] CreateEventDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var entity = _mapper.Map<Event>(dto);
            await _eventService.AddEventAsync(entity);
            var created = _mapper.Map<EventDto>(entity);
            return CreatedAtAction(nameof(GetEvent), new { encodedId = IdEncoder.EncodeId(entity.Id) }, created);
        }

        [HttpPut("{encodedId}")]
        public async Task<ActionResult> UpdateEvent(string encodedId, [FromBody] UpdateEventDto dto)
        {
            if (!IdEncoding.TryDecode(encodedId, out var id))
                return BadRequest("Invalid event ID");
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existing = await _eventService.GetEventAsync(id);
            if (existing == null)
                return NotFound();

            _mapper.Map(dto, existing);
            await _eventService.UpdateEventAsync(existing);
            return NoContent();
        }

        [HttpDelete("{encodedId}")]
        public async Task<ActionResult> DeleteEvent(string encodedId)
        {
            if (!IdEncoding.TryDecode(encodedId, out var id))
                return BadRequest("Invalid event ID");

            var entity = await _eventService.GetEventAsync(id);
            if (entity == null)
                return NotFound();

            await _eventService.DeleteEventAsync(id);
            return NoContent();
        }
    }
}
