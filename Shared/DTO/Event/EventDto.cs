using Shared.DTO.File;

namespace Shared.DTO.Event
{
    public class EventDto
    {
        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string NameAr { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string DescriptionAr { get; set; } = null!;
        public string Location { get; set; } = null!;
        public string LocationAr { get; set; } = null!;
        public DateTime EventDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal TicketPrice { get; set; }
        public string? PriceRange { get; set; }
        public int TotalTickets { get; set; }
        public bool IsActive { get; set; }
        public List<FileDto> Images { get; set; } = new();
    }
}
