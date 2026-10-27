namespace PlazaCore.Entites
{
    public class Trip : BaseEntity
    {
        public string Name { get; set; } = null!;
        public string NameAr { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string DescriptionAr { get; set; } = null!;
        public string City { get; set; } = null!;
        public string CityAr { get; set; } = null!;
        public string Itinerary { get; set; } = null!;
        public string ItineraryAr { get; set; } = null!;
        public int DurationDays { get; set; }
        public int DurationHours { get; set; }
        public decimal PricePerPerson { get; set; }
        public decimal PricePerChild { get; set; }
        public int MaxCapacity { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsActive { get; set; } = true;
        public string? ImageIds { get; set; }

        public ICollection<PackageTrip> PackageTrips { get; set; } = new HashSet<PackageTrip>();
    }
}
