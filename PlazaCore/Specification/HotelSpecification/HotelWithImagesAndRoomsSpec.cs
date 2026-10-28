using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PlazaCore.Entites;

namespace PlazaCore.Specification.HotelSpecification
{
    public class HotelWithImagesAndRoomsSpec : Specification<Hotel>
    {
        public HotelWithImagesAndRoomsSpec()
        {
            AddInclude(x => x.Rooms);
            AddInclude(x => x.InsightHotel);
            
        }
        public HotelWithImagesAndRoomsSpec(int id):base( h => h.Id==id)
        {
            AddInclude(x => x.Rooms);
            AddInclude(x => x.InsightHotel);

        }

        public HotelWithImagesAndRoomsSpec(decimal? minPrice, decimal? maxPrice)
        {
            AddInclude(x => x.Rooms);
            AddInclude(x => x.InsightHotel);

            if (minPrice != null && maxPrice != null)
                Criteria = hotel => hotel.Rooms.Any(room => room.PricePerNight >= minPrice && room.PricePerNight <= maxPrice);
            else if (minPrice != null)
                Criteria = hotel => hotel.Rooms.Any(room => room.PricePerNight >= minPrice);
            else if (maxPrice != null)
                Criteria = hotel => hotel.Rooms.Any(room => room.PricePerNight <= maxPrice);
        }
    }
}
