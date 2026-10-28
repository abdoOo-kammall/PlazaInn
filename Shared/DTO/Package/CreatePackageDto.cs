using System.ComponentModel.DataAnnotations;

namespace Shared.DTO.Package
{
    public class CreatePackageDto : IValidatableObject
    {
        [Required(ErrorMessage = "Name (English) is required")]
        [StringLength(150, ErrorMessage = "Name cannot exceed 150 characters")]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "Name (Arabic) is required")]
        [StringLength(150, ErrorMessage = "Name (Arabic) cannot exceed 150 characters")]
        public string NameAr { get; set; } = null!;

        [Required(ErrorMessage = "Description (English) is required")]
        [StringLength(4000, ErrorMessage = "Description cannot exceed 4000 characters")]
        public string Description { get; set; } = null!;

        [Required(ErrorMessage = "Description (Arabic) is required")]
        [StringLength(4000, ErrorMessage = "Description (Arabic) cannot exceed 4000 characters")]
        public string DescriptionAr { get; set; } = null!;

        [Range(1, 365, ErrorMessage = "Duration days must be at least 1")]
        public int DurationDays { get; set; }

        [Range(typeof(decimal), "0.01", "999999999999.99", ErrorMessage = "Package price must be greater than 0")]
        public decimal PackagePrice { get; set; }

        [StringLength(200, ErrorMessage = "Price range cannot exceed 200 characters")]
        public string? PriceRange { get; set; }

        [Range(1, 100000, ErrorMessage = "Max packages available must be at least 1")]
        public int MaxPackagesAvailable { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsActive { get; set; } = true;

        [MinLength(1, ErrorMessage = "At least one image ID is required")]
        public List<int> ImageIds { get; set; } = new();

        public string? HotelId { get; set; }
        public string? RoomId { get; set; }

        [Range(1, 365, ErrorMessage = "Nights count must be at least 1")]
        public int? NightsCount { get; set; }

        public List<string> TripIds { get; set; } = new();
        public List<string> TransportIds { get; set; } = new();
        public List<string> EventIds { get; set; } = new();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (EndDate.HasValue && !StartDate.HasValue)
            {
                yield return new ValidationResult(
                    "Start date is required when an end date is set.",
                    new[] { nameof(StartDate) });
            }

            if (StartDate.HasValue && EndDate.HasValue && EndDate < StartDate)
            {
                yield return new ValidationResult(
                    "End date must be on or after the start date.",
                    new[] { nameof(EndDate) });
            }

            var hasHotel = !string.IsNullOrWhiteSpace(HotelId);
            var hasRoom = !string.IsNullOrWhiteSpace(RoomId);
            var hasNights = NightsCount.HasValue;
            if (hasHotel || hasRoom || hasNights)
            {
                if (!hasHotel || !hasRoom || !hasNights)
                {
                    yield return new ValidationResult(
                        "Hotel, room, and nights must be provided together.",
                        new[] { nameof(HotelId), nameof(RoomId), nameof(NightsCount) });
                }
            }

            var hasTrip = TripIds.Any(id => !string.IsNullOrWhiteSpace(id));
            var hasTransport = TransportIds.Any(id => !string.IsNullOrWhiteSpace(id));
            var hasEvent = EventIds.Any(id => !string.IsNullOrWhiteSpace(id));
            if (!hasHotel && !hasTrip && !hasTransport && !hasEvent)
            {
                yield return new ValidationResult(
                    "A package must include at least one item.",
                    new[] { nameof(TripIds), nameof(TransportIds), nameof(EventIds), nameof(HotelId) });
            }
        }
    }
}
