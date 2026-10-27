using PlazaCore.Entites;
using Shared.Enums;

namespace PlazaCore.Specification.BookingSpecification
{
    /// <summary>
    /// Confirmed bookings, and pending bookings still inside the payment hold, for one catalog item.
    /// Cancelled bookings and expired pending holds are excluded so they do not consume capacity.
    /// </summary>
    public class ActiveTripBookingsSpec : Specification<Booking>
    {
        public ActiveTripBookingsSpec(int tripId, DateTime serviceDate, DateTime pendingCutoff, int? excludeBookingId = null)
        {
            var day = serviceDate.Date;
            var nextDay = day.AddDays(1);
            Criteria = b =>
                b.BookingType == BookingType.Trip &&
                b.TripId == tripId &&
                b.ServiceDate >= day &&
                b.ServiceDate < nextDay &&
                b.Status != BookingStatus.Cancelled &&
                (b.Status != BookingStatus.Pending || b.CreatedAt >= pendingCutoff) &&
                (excludeBookingId == null || b.Id != excludeBookingId);
        }
    }

    public class ActiveTransportBookingsSpec : Specification<Booking>
    {
        public ActiveTransportBookingsSpec(int transportId, DateTime serviceDate, DateTime pendingCutoff, int? excludeBookingId = null)
        {
            var day = serviceDate.Date;
            var nextDay = day.AddDays(1);
            Criteria = b =>
                b.BookingType == BookingType.Transport &&
                b.TransportId == transportId &&
                b.ServiceDate >= day &&
                b.ServiceDate < nextDay &&
                b.Status != BookingStatus.Cancelled &&
                (b.Status != BookingStatus.Pending || b.CreatedAt >= pendingCutoff) &&
                (excludeBookingId == null || b.Id != excludeBookingId);
        }
    }

    public class ActiveEventBookingsSpec : Specification<Booking>
    {
        public ActiveEventBookingsSpec(int eventId, DateTime pendingCutoff, int? excludeBookingId = null)
        {
            Criteria = b =>
                b.BookingType == BookingType.Event &&
                b.EventId == eventId &&
                b.Status != BookingStatus.Cancelled &&
                (b.Status != BookingStatus.Pending || b.CreatedAt >= pendingCutoff) &&
                (excludeBookingId == null || b.Id != excludeBookingId);
        }
    }

    public class ActivePackageBookingsSpec : Specification<Booking>
    {
        public ActivePackageBookingsSpec(int packageId, DateTime pendingCutoff, int? excludeBookingId = null)
        {
            Criteria = b =>
                b.BookingType == BookingType.Package &&
                b.PackageId == packageId &&
                b.Status != BookingStatus.Cancelled &&
                (b.Status != BookingStatus.Pending || b.CreatedAt >= pendingCutoff) &&
                (excludeBookingId == null || b.Id != excludeBookingId);
        }
    }
}
