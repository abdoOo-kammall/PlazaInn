using Shared.DTO.Image;

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
        public int Phone { get; set; }
        public string Email { get; set; }

        //  Hotel Rating
        public double Rating { get; set; }

        //  Calculated fields
        public int NumOfRooms { get; set; }
        public int NumOfSuites { get; set; }

        //  Image list
        public List<ImageDTO> Images { get; set; } = new();
    }
}
