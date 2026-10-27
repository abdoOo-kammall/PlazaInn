using System.ComponentModel.DataAnnotations;

namespace Shared.DTO.Trip
{
    public class CreateTripDto : IValidatableObject
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

        [Required(ErrorMessage = "City (English) is required")]
        [StringLength(100, ErrorMessage = "City cannot exceed 100 characters")]
        public string City { get; set; } = null!;

        [Required(ErrorMessage = "City (Arabic) is required")]
        [StringLength(100, ErrorMessage = "City (Arabic) cannot exceed 100 characters")]
        public string CityAr { get; set; } = null!;

        [Required(ErrorMessage = "Itinerary (English) is required")]
        [StringLength(8000, ErrorMessage = "Itinerary cannot exceed 8000 characters")]
        public string Itinerary { get; set; } = null!;

        [Required(ErrorMessage = "Itinerary (Arabic) is required")]
        [StringLength(8000, ErrorMessage = "Itinerary (Arabic) cannot exceed 8000 characters")]
        public string ItineraryAr { get; set; } = null!;

        [Range(0, 365, ErrorMessage = "Duration days must be between 0 and 365")]
        public int DurationDays { get; set; }

        [Range(0, 8784, ErrorMessage = "Duration hours must be between 0 and 8784")]
        public int DurationHours { get; set; }

        [Range(typeof(decimal), "0.01", "999999999999.99", ErrorMessage = "Price per person must be greater than 0")]
        public decimal PricePerPerson { get; set; }

        [Range(typeof(decimal), "0.01", "999999999999.99", ErrorMessage = "Price per child must be greater than 0")]
        public decimal PricePerChild { get; set; }

        [Range(1, 100000, ErrorMessage = "Max capacity must be at least 1")]
        public int MaxCapacity { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsActive { get; set; } = true;

        [MinLength(1, ErrorMessage = "At least one image ID is required")]
        public List<int> ImageIds { get; set; } = new();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (DurationDays <= 0 && DurationHours <= 0)
            {
                yield return new ValidationResult(
                    "Duration must be at least 1 hour or 1 day.",
                    new[] { nameof(DurationDays), nameof(DurationHours) });
            }

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
        }
    }
}
