using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlazaCore.Entites
{
    public class ContactUs :BaseEntity
    {
        public string? Email { get; set; }
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public string? Instagram { get; set; }
        public string? Facebook { get; set; }
        public string? WhatsApp { get; set; }
        public string? LinkedIn { get; set; }
        public ICollection<WorkingHour> WorkingHours { get; set; }= new HashSet<WorkingHour>();

    }
}
