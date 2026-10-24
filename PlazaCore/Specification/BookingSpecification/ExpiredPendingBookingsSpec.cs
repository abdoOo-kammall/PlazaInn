using PlazaCore.Entites;
using Shared.Enums;

namespace PlazaCore.Specification.BookingSpecification
{
    public class ExpiredPendingBookingsSpec : Specification<Booking>
    {
        public ExpiredPendingBookingsSpec(DateTime cutoff)
            : base(b => b.Status == BookingStatus.Pending && b.CreatedAt < cutoff)
        {
        }
    }
}
