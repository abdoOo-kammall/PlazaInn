namespace Shared.DTO.Hotel
{
    public class HotelDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public double Rating { get; set; }
        //public ICollection<HotelImageDto> HotelImages { get; set; }
        public List<string> ImageUrls { get; set; } = new();


    }
}
