using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PlazaCore.Entites;

namespace PlazaCore.Specification.RoomSpecification
{
    public class RoomByHotelIdSpec : Specification<Room>
    {
        public RoomByHotelIdSpec(int hotelId):base (r=>r.HotelId == hotelId)
        {
            AddInclude(X => X.Hotel);
        }
    }
}
