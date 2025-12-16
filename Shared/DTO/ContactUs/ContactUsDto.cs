using Shared.DTO.WorkingHours;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.DTO.ContactUs
{
    public class ContactUsDto
    {
        public int Id { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public string? Phone { get; set; }

        public string? Instagram { get; set; }
        public string? Facebook { get; set; }
        public string? WhatsApp { get; set; }
        public string? LinkedIn { get; set; }
        public List<WorkingHourDto> workingHours { get; set; }
    }
}
