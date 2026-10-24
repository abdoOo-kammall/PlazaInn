using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared.Helper;

namespace Shared.DTO.Booking
{
    public class BookingQuoteRequestDto : IValidatableObject
    {
        [Required]
        public string EncodedHotelId { get; set; } = null!;

        [Required]
        public string EncodedRoomId { get; set; } = null!;

        [Range(1, 20, ErrorMessage = "Rooms count must be between 1 and 20.")]
        public int RoomsCount { get; set; } = 1;

        [Range(1, 50, ErrorMessage = "Adults must be between 1 and 50.")]
        public int Adults { get; set; } = 1;

        [Range(0, 50, ErrorMessage = "Children must be between 0 and 50.")]
        public int Children { get; set; }

        public DateTime CheckIn { get; set; }
        public DateTime CheckOut { get; set; }

        public bool ExtraBed { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var error = StayRules.GetStayLengthError(CheckIn, CheckOut);
            if (error != null)
                yield return new ValidationResult(error, new[] { nameof(CheckIn), nameof(CheckOut) });
        }
    }

    public class BookingRequestDto : BookingQuoteRequestDto
    {
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string CustomerName { get; set; } = null!;

        [Required]
        [EmailAddress]
        public string CustomerEmail { get; set; } = null!;

        [Required]
        [Phone]
        [StringLength(32)]
        public string CustomerPhone { get; set; } = null!;
    }

}
