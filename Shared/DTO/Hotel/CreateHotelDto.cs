using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Shared.DTO.InsightHotel;

namespace Shared.DTO.Hotel
{
    public class CreateHotelDto
    {
        [Required(ErrorMessage = "Hotel name (English) is required")]
        [StringLength(100, ErrorMessage = "Hotel name cannot exceed 100 characters")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Hotel name (Arabic) is required")]
        [StringLength(100, ErrorMessage = "Hotel name (Arabic) cannot exceed 100 characters")]
        public string NameAr { get; set; }

        [Required(ErrorMessage = "Description (English) is required")]
        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Description (Arabic) is required")]
        [StringLength(500, ErrorMessage = "Description (Arabic) cannot exceed 500 characters")]
        public string DescriptionAr { get; set; }

        [Required(ErrorMessage = "Address (English) is required")]
        [StringLength(200, ErrorMessage = "Address cannot exceed 200 characters")]
        public string Address { get; set; }

        [Required(ErrorMessage = "Address (Arabic) is required")]
        [StringLength(200, ErrorMessage = "Address (Arabic) cannot exceed 200 characters")]
        public string AddressAr { get; set; }

        [Required(ErrorMessage = "City (English) is required")]
        [StringLength(50, ErrorMessage = "City cannot exceed 50 characters")]
        public string City { get; set; }

        [Required(ErrorMessage = "City (Arabic) is required")]
        [StringLength(50, ErrorMessage = "City (Arabic) cannot exceed 50 characters")]
        public string CityAr { get; set; }

        [Required(ErrorMessage = "Location is required")]
        [Url(ErrorMessage = "Location must be a valid URL")]
        public string Location { get; set; }

        [Required(ErrorMessage = "Phone number is required")]
        [RegularExpression(@"^\d{6,15}$", ErrorMessage = "Phone number must contain only digits and be between 6 and 15 digits")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address format")]
        public string Email { get; set; }


        public int NumOfAvailableRoomsToReserve { get; set; } = 0;
        public int NumOfAvailableSuitesToReserve { get; set; } = 0;
        // Rating is optional on creation (you can default it to 0)
        public double Rating { get; set; } = 0;
        // socail media 
        public string? Instagram { get; set; }
        public string? Facebook { get; set; }
        public string? WhatsApp { get; set; }
        public string? Space { get; set; }
        //public InsightHotelDto? Insight { get; set; }


        [MinLength(1, ErrorMessage = "At least one image ID is required")]
        public List<int> ImageIds { get; set; } = new();
    }
}
