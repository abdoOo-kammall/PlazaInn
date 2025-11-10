using Shared.DTO.Image;
using Shared.DTO.InsightHotel;

namespace Shared.DTO.Hotel
{
    public class HotelDto
    {
        public string Id { get; set; }

        //  Names
        public string Name { get; set; }
        public string NameAr { get; set; }

        //  Descriptions
        public string Description { get; set; }
        public string DescriptionAr { get; set; }

        //  Addresses
        public string Address { get; set; }
        public string AddressAr { get; set; }

        //  City
        public string City { get; set; }
        public string CityAr { get; set; }

        //  Map URL
        public string Location { get; set; }

        //  Contact Info
        public string Phone { get; set; }
        public string Email { get; set; }

        //  Hotel Rating
        public double Rating { get; set; }

        // socail media 
        public string? Instagram { get; set; }
        public string? Facebook { get; set; }
        public string? WhatsApp { get; set; }
        public string? Space { get; set; }

        //  Calculated fields
        //public int NumOfAllRooms { get; set; }
        //public int NumOfAllSuites { get; set; }

        public int NumOfAvailableRoomsToReserve { get; set; }
        public int NumOfAvailableSuitesToReserve { get; set; }
        //  Image list
        public List<ImageDTO> Images { get; set; } = new();

        public InsightHotelDto? Insight { get; set; }

    }
}
