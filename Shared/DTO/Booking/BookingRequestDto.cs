using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared.Enums;
using Shared.Helper;

namespace Shared.DTO.Booking
{
    public class BookingQuoteRequestDto : IValidatableObject
    {
        public string? EncodedHotelId { get; set; }

        public string? EncodedRoomId { get; set; }

        [Range(1, 20, ErrorMessage = "Rooms count must be between 1 and 20.")]
        public int RoomsCount { get; set; } = 1;

        [Range(1, 50, ErrorMessage = "Adults must be between 1 and 50.")]
        public int Adults { get; set; } = 1;

        [Range(0, 50, ErrorMessage = "Children must be between 0 and 50.")]
        public int Children { get; set; }

        public DateTime CheckIn { get; set; }
        public DateTime CheckOut { get; set; }

        public bool ExtraBed { get; set; }

        public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrWhiteSpace(EncodedHotelId))
                yield return new ValidationResult("The EncodedHotelId field is required.", new[] { nameof(EncodedHotelId) });

            if (string.IsNullOrWhiteSpace(EncodedRoomId))
                yield return new ValidationResult("The EncodedRoomId field is required.", new[] { nameof(EncodedRoomId) });

            var error = StayRules.GetStayLengthError(CheckIn, CheckOut);
            if (error != null)
                yield return new ValidationResult(error, new[] { nameof(CheckIn), nameof(CheckOut) });
        }
    }

    public class BookingRequestDto : BookingQuoteRequestDto
    {
        public BookingType BookingType { get; set; } = BookingType.Hotel;

        public string? EncodedTripId { get; set; }
        public string? EncodedTransportId { get; set; }
        public string? EncodedEventId { get; set; }
        public string? EncodedPackageId { get; set; }

        public DateTime? ServiceDate { get; set; }

        [Range(1, 1000, ErrorMessage = "Quantity must be between 1 and 1000.")]
        public int Quantity { get; set; } = 1;

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

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (BookingType == BookingType.Hotel)
            {
                foreach (var result in base.Validate(validationContext))
                    yield return result;
                yield break;
            }

            switch (BookingType)
            {
                case BookingType.Trip:
                    if (string.IsNullOrWhiteSpace(EncodedTripId))
                        yield return new ValidationResult("Trip is required.", new[] { nameof(EncodedTripId) });
                    if (!ServiceDate.HasValue)
                        yield return new ValidationResult("Service date is required.", new[] { nameof(ServiceDate) });
                    if (Adults + Children < 1)
                        yield return new ValidationResult("At least one guest is required.", new[] { nameof(Adults), nameof(Children) });
                    break;
                case BookingType.Transport:
                    if (string.IsNullOrWhiteSpace(EncodedTransportId))
                        yield return new ValidationResult("Transport is required.", new[] { nameof(EncodedTransportId) });
                    if (!ServiceDate.HasValue)
                        yield return new ValidationResult("Service date is required.", new[] { nameof(ServiceDate) });
                    break;
                case BookingType.Event:
                    if (string.IsNullOrWhiteSpace(EncodedEventId))
                        yield return new ValidationResult("Event is required.", new[] { nameof(EncodedEventId) });
                    break;
                case BookingType.Package:
                    if (string.IsNullOrWhiteSpace(EncodedPackageId))
                        yield return new ValidationResult("Package is required.", new[] { nameof(EncodedPackageId) });
                    break;
                default:
                    yield return new ValidationResult("Unsupported booking type.", new[] { nameof(BookingType) });
                    break;
            }
        }
    }

}
