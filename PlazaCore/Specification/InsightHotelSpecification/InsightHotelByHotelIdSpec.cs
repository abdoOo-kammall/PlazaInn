using PlazaCore.Entites;

namespace PlazaCore.Specification.InsightHotelSpecification
{
    public class InsightHotelByHotelIdSpec : Specification<InsightHotel>
    {
        public InsightHotelByHotelIdSpec(int hotelId)
            : base(i => i.HotelId == hotelId)
        {
        }
    }
}
