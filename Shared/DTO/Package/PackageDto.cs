using Shared.DTO.File;

namespace Shared.DTO.Package
{
    public class PackageDto
    {
        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string NameAr { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string DescriptionAr { get; set; } = null!;
        public int DurationDays { get; set; }
        public decimal PackagePrice { get; set; }
        public string? PriceRange { get; set; }
        public int MaxPackagesAvailable { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsActive { get; set; }
        public List<FileDto> Images { get; set; } = new();

        public string? HotelId { get; set; }
        public string? HotelName { get; set; }
        public string? HotelNameAr { get; set; }
        public string? RoomId { get; set; }
        public string? RoomName { get; set; }
        public string? RoomNameAr { get; set; }
        public int? NightsCount { get; set; }

        public List<PackageComponentDto> Trips { get; set; } = new();
        public List<PackageComponentDto> Transports { get; set; } = new();
        public List<PackageComponentDto> Events { get; set; } = new();
    }
}
