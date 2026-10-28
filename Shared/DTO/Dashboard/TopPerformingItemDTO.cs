namespace Shared.DTO.Dashboard
{
    public class TopPerformingItemDTO
    {
        public string EncodedId { get; set; } = "";
        public string Name { get; set; } = "";
        public int BookingsCount { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class TopDestinationDTO
    {
        public string Location { get; set; } = "";
        public int BookingsCount { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class TopPerformersDTO
    {
        public List<TopPerformingItemDTO> Hotels { get; set; } = new();
        public List<TopPerformingItemDTO> Trips { get; set; } = new();
        public List<TopPerformingItemDTO> Transports { get; set; } = new();
        public List<TopPerformingItemDTO> Events { get; set; } = new();
        public List<TopPerformingItemDTO> Packages { get; set; } = new();
        public List<TopDestinationDTO> TopDestinations { get; set; } = new();
    }
}
