using PlazaCore.Entites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlazaCore.Specification.BookingSpecification
{
    public class BookingInDateRangeSpec : Specification<Booking>
    {
       
            public BookingInDateRangeSpec(int roomId, DateTime checkIn, DateTime checkOut)
            {
                Criteria = b =>
                    b.RoomId == roomId &&
                    b.CheckIn < checkOut &&
                    b.CheckOut > checkIn;

                AddInclude(b => b.Room);
                AddInclude(b => b.Hotel);
            }
        
    }
}
