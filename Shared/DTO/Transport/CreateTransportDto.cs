using System.ComponentModel.DataAnnotations;

namespace Shared.DTO.Transport
{
    public class CreateTransportDto
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

        [Required(ErrorMessage = "Vehicle type (English) is required")]
        [StringLength(100, ErrorMessage = "Vehicle type cannot exceed 100 characters")]
        public string VehicleType { get; set; } = null!;

        [Required(ErrorMessage = "Vehicle type (Arabic) is required")]
        [StringLength(100, ErrorMessage = "Vehicle type (Arabic) cannot exceed 100 characters")]
        public string VehicleTypeAr { get; set; } = null!;

        [Required(ErrorMessage = "Pickup location (English) is required")]
        [StringLength(250, ErrorMessage = "Pickup location cannot exceed 250 characters")]
        public string PickupLocation { get; set; } = null!;

        [Required(ErrorMessage = "Pickup location (Arabic) is required")]
        [StringLength(250, ErrorMessage = "Pickup location (Arabic) cannot exceed 250 characters")]
        public string PickupLocationAr { get; set; } = null!;

        [Required(ErrorMessage = "Dropoff location (English) is required")]
        [StringLength(250, ErrorMessage = "Dropoff location cannot exceed 250 characters")]
        public string DropoffLocation { get; set; } = null!;

        [Required(ErrorMessage = "Dropoff location (Arabic) is required")]
        [StringLength(250, ErrorMessage = "Dropoff location (Arabic) cannot exceed 250 characters")]
        public string DropoffLocationAr { get; set; } = null!;

        [Range(1, 1000, ErrorMessage = "Max passengers must be at least 1")]
        public int MaxPassengers { get; set; }

        [Range(0, 1000, ErrorMessage = "Max luggage cannot be negative")]
        public int MaxLuggage { get; set; }

        [Range(typeof(decimal), "0.01", "999999999999.99", ErrorMessage = "Price per trip must be greater than 0")]
        public decimal PricePerTrip { get; set; }

        [Range(1, 100000, ErrorMessage = "Available vehicles must be at least 1")]
        public int AvailableVehicles { get; set; }

        public bool IsActive { get; set; } = true;

        [MinLength(1, ErrorMessage = "At least one image ID is required")]
        public List<int> ImageIds { get; set; } = new();
    }
}
