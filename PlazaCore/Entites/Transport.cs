namespace PlazaCore.Entites
{
    public class Transport : BaseEntity
    {
        public string Name { get; set; } = null!;
        public string NameAr { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string DescriptionAr { get; set; } = null!;
        public string VehicleType { get; set; } = null!;
        public string VehicleTypeAr { get; set; } = null!;
        public string PickupLocation { get; set; } = null!;
        public string PickupLocationAr { get; set; } = null!;
        public string DropoffLocation { get; set; } = null!;
        public string DropoffLocationAr { get; set; } = null!;
        public int MaxPassengers { get; set; }
        public int MaxLuggage { get; set; }
        public decimal PricePerTrip { get; set; }
        public int AvailableVehicles { get; set; }
        public bool IsActive { get; set; } = true;
        public string? ImageIds { get; set; }

        public ICollection<PackageTransport> PackageTransports { get; set; } = new HashSet<PackageTransport>();
    }
}
