using Microsoft.AspNetCore.Http;

namespace Shared.DTO.Hotel
{
    public class CreateHotelDto
    {


        public string Name { get; set; }
        public string Description { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        //public double Rating { get; set; }
        public List<int> ImageIds { get; set; } = new();

    }

}
