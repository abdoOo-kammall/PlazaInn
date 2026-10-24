using PlazaCore.Entites;
using Shared.Enums;

namespace PlazaCore.Specification.BookingSpecification
{
    public class BookingFilterSpec : Specification<Booking>
    {
        /// <param name="userId">When null, bookings of all users are returned (admin view).</param>
        public BookingFilterSpec(string? userId, int? hotelId, DateTime? from, DateTime? to, BookingStatus? status)
        {
            Criteria = b =>
                (userId == null || b.UserId == userId) &&
                (!hotelId.HasValue || b.HotelId == hotelId.Value) &&
                (!from.HasValue || b.CheckIn >= from.Value) &&
                (!to.HasValue || b.CheckOut <= to.Value) &&
                (!status.HasValue || b.Status == status.Value);

            ApplyOrderByDesc(b => b.CreatedAt);
        }
    }
}
