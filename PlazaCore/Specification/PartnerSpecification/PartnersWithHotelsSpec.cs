using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PlazaCore.Entites;

namespace PlazaCore.Specification.PartnerSpecification
{
    public class PartnersWithHotelsSpec : Specification<Partners>
    {
        public PartnersWithHotelsSpec()
        {
            AddInclude(p => p.Hotels);
        }
    }
}
