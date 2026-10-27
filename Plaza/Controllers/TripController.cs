using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Plaza.Extensions;
using PlazaCore.Entites;
using PlazaCore.ServiceContract;
using Shared.DTO.Trip;
using Shared.Security;

namespace Plaza.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class TripController : ControllerBase
    {
        private readonly ITripService _tripService;
        private readonly IMapper _mapper;

        public TripController(ITripService tripService, IMapper mapper)
        {
            _tripService = tripService;
            _mapper = mapper;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<TripDto>>> GetTrips([FromQuery] TripFilterDto filter)
        {
            var trips = await _tripService.GetTripsAsync(filter, activeOnly: !User.IsAdmin());
            return Ok(_mapper.Map<IEnumerable<TripDto>>(trips));
        }

        [HttpGet("lookup")]
        [AllowAnonymous]
        public async Task<IActionResult> GetLookup()
        {
            var trips = await _tripService.GetTripsAsync(new TripFilterDto(), activeOnly: !User.IsAdmin());
            var result = trips.Select(trip => new
            {
                Id = IdEncoder.EncodeId(trip.Id),
                Name = trip.Name,
                NameAr = trip.NameAr
            });
            return Ok(result);
        }

        [HttpGet("{encodedId}")]
        [AllowAnonymous]
        public async Task<ActionResult<TripDto>> GetTrip(string encodedId)
        {
            if (!IdEncoding.TryDecode(encodedId, out var id))
                return BadRequest("Invalid trip ID");

            var trip = await _tripService.GetTripAsync(id);
            if (trip == null || (!trip.IsActive && !User.IsAdmin()))
                return NotFound();

            return Ok(_mapper.Map<TripDto>(trip));
        }

        [HttpPost]
        public async Task<ActionResult> AddTrip([FromBody] CreateTripDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var trip = _mapper.Map<Trip>(dto);
            await _tripService.AddTripAsync(trip);
            var created = _mapper.Map<TripDto>(trip);
            return CreatedAtAction(nameof(GetTrip), new { encodedId = IdEncoder.EncodeId(trip.Id) }, created);
        }

        [HttpPut("{encodedId}")]
        public async Task<ActionResult> UpdateTrip(string encodedId, [FromBody] UpdateTripDto dto)
        {
            if (!IdEncoding.TryDecode(encodedId, out var id))
                return BadRequest("Invalid trip ID");
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existing = await _tripService.GetTripAsync(id);
            if (existing == null)
                return NotFound();

            _mapper.Map(dto, existing);
            await _tripService.UpdateTripAsync(existing);
            return NoContent();
        }

        [HttpDelete("{encodedId}")]
        public async Task<ActionResult> DeleteTrip(string encodedId)
        {
            if (!IdEncoding.TryDecode(encodedId, out var id))
                return BadRequest("Invalid trip ID");

            var trip = await _tripService.GetTripAsync(id);
            if (trip == null)
                return NotFound();

            await _tripService.DeleteTripAsync(id);
            return NoContent();
        }
    }
}
