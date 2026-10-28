namespace PlazaCore.Entites
{
    public class Event : BaseEntity
    {
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
        public bool IsActive { get; set; } = true;
        public string? ImageIds { get; set; }

        public ICollection<PackageEvent> PackageEvents { get; set; } = new HashSet<PackageEvent>();
    }
}
