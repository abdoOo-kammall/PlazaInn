using PlazaCore.Entites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlazaCore.Specification.ContactUsSpec
{
    public class ContactUsSpecification :Specification<ContactUs>
    {
        public ContactUsSpecification()
        {
            AddInclude(c => c.WorkingHours);
        }
    }
}
