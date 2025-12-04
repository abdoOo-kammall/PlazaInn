using PlazaCore.Entites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlazaCore.Specification.BookingSpecification
{
    public class BookingByPaymentRefSpec : Specification<Booking>
    {
        public BookingByPaymentRefSpec(string paymentRef)
        {
            Criteria = b => b.PaymentReference == paymentRef;

            AddInclude(b => b.Room);
            AddInclude(b => b.Hotel);
        }
    }
}
