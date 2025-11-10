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
    }
}
