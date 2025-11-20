using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PlazaCore.Entites;

namespace PlazaCore.Specification.PartnerSpecification
{
    public class PartnerByIdWithHotelsSpec : Specification<Partners>
    {
        public PartnerByIdWithHotelsSpec(int id) : base(p => p.Id == id)
        {
            AddInclude(p => p.Hotels);
            //Criteria = p => p.Id == id;
        }
    }
}
