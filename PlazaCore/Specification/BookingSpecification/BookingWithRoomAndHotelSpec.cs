using PlazaCore.Entites;

namespace PlazaCore.Specification.BookingSpecification
{
    public class BookingWithRoomAndHotelSpec : Specification<Booking>
    {
        public BookingWithRoomAndHotelSpec(int bookingId) : base(b => b.Id == bookingId)
        {
            AddInclude(b => b.Room);
            AddInclude(b => b.Hotel);
        }
    }
}
