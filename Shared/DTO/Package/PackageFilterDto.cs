using System.ComponentModel.DataAnnotations;

namespace Shared.DTO.Package
{
    public class PackageFilterDto : IValidatableObject
    {
        [StringLength(150)]
        public string? Search { get; set; }

        public DateTime? Date { get; set; }

        [Range(typeof(decimal), "0", "999999999999.99")]
        public decimal? MinPrice { get; set; }

        [Range(typeof(decimal), "0", "999999999999.99")]
        public decimal? MaxPrice { get; set; }

        [Range(1, 365)]
        public int? DurationDays { get; set; }

        public bool? IncludesHotel { get; set; }
        public bool? IsActive { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (MinPrice.HasValue && MaxPrice.HasValue && MinPrice > MaxPrice)
            {
                yield return new ValidationResult(
                    "Min price cannot exceed max price.",
                    new[] { nameof(MinPrice), nameof(MaxPrice) });
            }
        }
    }
}
