using PlazaCore.Entites;
using Shared.Enums;

namespace PlazaCore.Specification.BookingSpecification
{
    public class BookingFilterSpec : Specification<Booking>
    {
        /// <param name="userId">When null, bookings of all users are returned (admin view).</param>
        public BookingFilterSpec(string? userId, int? hotelId, DateTime? from, DateTime? to, BookingStatus? status, BookingType? bookingType = null)
        {
            Criteria = b =>
                (userId == null || b.UserId == userId) &&
                (!hotelId.HasValue || b.HotelId == hotelId.Value) &&
                (!bookingType.HasValue || b.BookingType == bookingType.Value) &&
                (!from.HasValue || b.CheckIn >= from.Value || (b.ServiceDate != null && b.ServiceDate >= from.Value)) &&
                (!to.HasValue || b.CheckOut <= to.Value || (b.ServiceDate != null && b.ServiceDate <= to.Value)) &&
                (!status.HasValue || b.Status == status.Value);

            ApplyOrderByDesc(b => b.CreatedAt);
            BookingWithRoomAndHotelSpec.AddBookingItemIncludes(this);
        }
    }
}
