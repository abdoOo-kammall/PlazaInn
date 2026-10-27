using PlazaCore.Entites;

namespace PlazaCore.Specification.BookingSpecification
{
    public class BookingWithRoomAndHotelSpec : Specification<Booking>
    {
        public BookingWithRoomAndHotelSpec(int bookingId) : base(b => b.Id == bookingId)
        {
            AddBookingItemIncludes(this);
        }

        internal static void AddBookingItemIncludes(Specification<Booking> spec)
        {
            spec.Includes.Add(b => b.Room!);
            spec.Includes.Add(b => b.Hotel!);
            spec.Includes.Add(b => b.Trip!);
            spec.Includes.Add(b => b.Transport!);
            spec.Includes.Add(b => b.Event!);
            spec.Includes.Add(b => b.Package!);
        }
    }
}
