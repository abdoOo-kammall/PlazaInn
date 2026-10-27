using System.ComponentModel.DataAnnotations;

namespace Shared.DTO.Event
{
    public class CreateEventDto : IValidatableObject
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

        [Required(ErrorMessage = "Location (English) is required")]
        [StringLength(250, ErrorMessage = "Location cannot exceed 250 characters")]
        public string Location { get; set; } = null!;

        [Required(ErrorMessage = "Location (Arabic) is required")]
        [StringLength(250, ErrorMessage = "Location (Arabic) cannot exceed 250 characters")]
        public string LocationAr { get; set; } = null!;

        [Required(ErrorMessage = "Event date is required")]
        public DateTime EventDate { get; set; }

        public DateTime? EndDate { get; set; }

        [Range(typeof(decimal), "0.01", "999999999999.99", ErrorMessage = "Ticket price must be greater than 0")]
        public decimal TicketPrice { get; set; }

        [Range(1, 1000000, ErrorMessage = "Total tickets must be at least 1")]
        public int TotalTickets { get; set; }

        public bool IsActive { get; set; } = true;

        [MinLength(1, ErrorMessage = "At least one image ID is required")]
        public List<int> ImageIds { get; set; } = new();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (EndDate.HasValue && EndDate < EventDate)
            {
                yield return new ValidationResult(
                    "End date must be on or after the event date.",
                    new[] { nameof(EndDate) });
            }
        }
    }
}
