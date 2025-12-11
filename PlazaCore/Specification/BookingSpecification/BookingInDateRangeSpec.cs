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
       
            public BookingInDateRangeSpec(int hotelId, DateTime checkIn, DateTime checkOut)
            {
                Criteria = b =>
                    b.HotelId == hotelId &&
                    b.CheckIn < checkOut &&
                    b.CheckOut > checkIn;

                AddInclude(b => b.Room);
                AddInclude(b => b.Hotel);
            }
        
    }
}
