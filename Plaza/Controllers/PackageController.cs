using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Plaza.Extensions;
using PlazaCore.Entites;
using PlazaCore.ServiceContract;
using Shared.DTO.Package;
using Shared.Security;

namespace Plaza.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class PackageController : ControllerBase
    {
        private readonly IPackageService _packageService;
        private readonly IMapper _mapper;

        public PackageController(IPackageService packageService, IMapper mapper)
        {
            _packageService = packageService;
            _mapper = mapper;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<PackageDto>>> GetPackages([FromQuery] PackageFilterDto filter)
        {
            var packages = await _packageService.GetPackagesAsync(filter, activeOnly: !User.IsAdmin());
            return Ok(_mapper.Map<IEnumerable<PackageDto>>(packages));
        }

        [HttpGet("lookup")]
        [AllowAnonymous]
        public async Task<IActionResult> GetLookup()
        {
            var packages = await _packageService.GetPackagesAsync(new PackageFilterDto(), activeOnly: !User.IsAdmin());
            var result = packages.Select(package => new
            {
                Id = IdEncoder.EncodeId(package.Id),
                Name = package.Name,
                NameAr = package.NameAr,
                PackagePrice = package.PackagePrice
            });
            return Ok(result);
        }

        [HttpGet("{encodedId}")]
        [AllowAnonymous]
        public async Task<ActionResult<PackageDto>> GetPackage(string encodedId)
        {
            if (!IdEncoding.TryDecode(encodedId, out var id))
                return BadRequest("Invalid package ID");

            var package = await _packageService.GetPackageAsync(id);
            if (package == null || (!package.IsActive && !User.IsAdmin()))
                return NotFound();

            return Ok(_mapper.Map<PackageDto>(package));
        }

        [HttpPost]
        public async Task<ActionResult> AddPackage([FromBody] CreatePackageDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            if (!TryReadComposition(dto, out var composition, out var error))
                return error!;

            var package = _mapper.Map<Package>(dto);
            await _packageService.AddPackageAsync(package, composition);
            var saved = await _packageService.GetPackageAsync(package.Id);
            var created = _mapper.Map<PackageDto>(saved);
            return CreatedAtAction(nameof(GetPackage), new { encodedId = IdEncoder.EncodeId(package.Id) }, created);
        }

        [HttpPut("{encodedId}")]
        public async Task<ActionResult> UpdatePackage(string encodedId, [FromBody] UpdatePackageDto dto)
        {
            if (!IdEncoding.TryDecode(encodedId, out var id))
                return BadRequest("Invalid package ID");
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            if (!TryReadComposition(dto, out var composition, out var error))
                return error!;

            var existing = await _packageService.GetPackageAsync(id);
            if (existing == null)
                return NotFound();

            _mapper.Map(dto, existing);
            await _packageService.UpdatePackageAsync(existing, composition);
            return NoContent();
        }

        [HttpDelete("{encodedId}")]
        public async Task<ActionResult> DeletePackage(string encodedId)
        {
            if (!IdEncoding.TryDecode(encodedId, out var id))
                return BadRequest("Invalid package ID");

            var package = await _packageService.GetPackageAsync(id);
            if (package == null)
                return NotFound();

            await _packageService.DeletePackageAsync(id);
            return NoContent();
        }

        private bool TryReadComposition(CreatePackageDto dto, out PackageComposition composition, out ActionResult? error)
        {
            composition = new PackageComposition();
            error = null;

            if (!TryOptionalId(dto.HotelId, "hotel", out var hotelId, out error)
                || !TryOptionalId(dto.RoomId, "room", out var roomId, out error)
                || !IdEncoding.TryDecodeMany(dto.TripIds, out var tripIds)
                || !IdEncoding.TryDecodeMany(dto.TransportIds, out var transportIds)
                || !IdEncoding.TryDecodeMany(dto.EventIds, out var eventIds))
            {
                error ??= BadRequest("Invalid package item ID");
                return false;
            }

            composition = new PackageComposition
            {
                HotelId = hotelId,
                RoomId = roomId,
                NightsCount = dto.NightsCount,
                TripIds = tripIds,
                TransportIds = transportIds,
                EventIds = eventIds
            };
            return true;
        }

        private bool TryOptionalId(string? encodedId, string resourceName, out int? id, out ActionResult? error)
        {
            if (string.IsNullOrWhiteSpace(encodedId))
            {
                id = null;
                error = null;
                return true;
            }

            if (!IdEncoding.TryDecode(encodedId, out var decoded))
            {
                id = null;
                error = BadRequest($"Invalid {resourceName} ID");
                return false;
            }

            id = decoded;
            error = null;
            return true;
        }
    }
}
