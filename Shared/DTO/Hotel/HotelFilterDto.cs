using System.ComponentModel.DataAnnotations;

namespace Shared.DTO.Hotel
{
    public class HotelFilterDto : IValidatableObject
    {
        [Range(typeof(decimal), "0", "999999999999.99")]
        public decimal? MinPrice { get; set; }

        [Range(typeof(decimal), "0", "999999999999.99")]
        public decimal? MaxPrice { get; set; }

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
