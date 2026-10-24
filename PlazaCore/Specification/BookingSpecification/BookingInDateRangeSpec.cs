using PlazaCore.Entites;
using Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlazaCore.Specification.BookingSpecification
{
    public class BookingInDateRangeSpec : Specification<Booking>
    {
        /// <summary>
        /// Active bookings (confirmed, or pending and still inside the payment hold window) that overlap the given stay.
        /// </summary>
        public BookingInDateRangeSpec(int hotelId, DateTime checkIn, DateTime checkOut, DateTime pendingCutoff, int? excludeBookingId = null)
        {
            Criteria = b =>
                b.HotelId == hotelId &&
                b.CheckIn < checkOut &&
                b.CheckOut > checkIn &&
                b.Status != BookingStatus.Cancelled &&
                (b.Status != BookingStatus.Pending || b.CreatedAt >= pendingCutoff) &&
                (excludeBookingId == null || b.Id != excludeBookingId);

            AddInclude(b => b.Room);
        }
    }
}
