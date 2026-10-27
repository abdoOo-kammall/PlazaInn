namespace PlazaCore.Entites
{
    public class Package : BaseEntity
    {
        public string Name { get; set; } = null!;
        public string NameAr { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string DescriptionAr { get; set; } = null!;
        public int DurationDays { get; set; }
        public decimal PackagePrice { get; set; }
        public int MaxPackagesAvailable { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsActive { get; set; } = true;
        public string? ImageIds { get; set; }

        public int? HotelId { get; set; }
        public Hotel? Hotel { get; set; }
        public int? RoomId { get; set; }
        public Room? Room { get; set; }
        public int? NightsCount { get; set; }

        public ICollection<PackageTrip> PackageTrips { get; set; } = new HashSet<PackageTrip>();
        public ICollection<PackageTransport> PackageTransports { get; set; } = new HashSet<PackageTransport>();
        public ICollection<PackageEvent> PackageEvents { get; set; } = new HashSet<PackageEvent>();
    }
}
