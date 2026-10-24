using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared.Helper;

namespace Shared.DTO.Booking
{
    public class RoomAvailabilityRequestDto : IValidatableObject
    {
        public DateTime CheckIn { get; set; }
        public DateTime CheckOut { get; set; }

        [Range(1, 20, ErrorMessage = "Rooms count must be between 1 and 20.")]
        public int RoomsCount { get; set; } = 1;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var error = StayRules.GetStayLengthError(CheckIn, CheckOut);
            if (error != null)
                yield return new ValidationResult(error, new[] { nameof(CheckIn), nameof(CheckOut) });
        }
    }
}
