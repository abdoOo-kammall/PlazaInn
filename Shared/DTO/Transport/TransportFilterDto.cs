using System.ComponentModel.DataAnnotations;

namespace Shared.DTO.Transport
{
    public class TransportFilterDto : IValidatableObject
    {
        [StringLength(150)]
        public string? Search { get; set; }

        [StringLength(100)]
        public string? VehicleType { get; set; }

        [StringLength(250)]
        public string? PickupLocation { get; set; }

        [StringLength(250)]
        public string? DropoffLocation { get; set; }

        [Range(1, 1000)]
        public int? Passengers { get; set; }

        [Range(typeof(decimal), "0", "999999999999.99")]
        public decimal? MinPrice { get; set; }

        [Range(typeof(decimal), "0", "999999999999.99")]
        public decimal? MaxPrice { get; set; }

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
