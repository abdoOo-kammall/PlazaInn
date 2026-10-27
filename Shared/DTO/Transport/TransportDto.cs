using Shared.DTO.File;

namespace Shared.DTO.Transport
{
    public class TransportDto
    {
        public string Id { get; set; } = null!;
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
        public bool IsActive { get; set; }
        public List<FileDto> Images { get; set; } = new();
    }
}
