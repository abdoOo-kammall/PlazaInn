using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PlazaCore.Entites;

namespace PlazaCore.Specification.PartnerSpecification
{
    public class PartnerByHotelIdSpec : Specification<Partners>
    {
        public PartnerByHotelIdSpec(int hotelId ) :base(p => p.Hotels.Any(h=>h.Id == hotelId))
        {
            //Criteria = p => p.Hotels.Any(h => h.Id == hotelId);

        }

    }
}
