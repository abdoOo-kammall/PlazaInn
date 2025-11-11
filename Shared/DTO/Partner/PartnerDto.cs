using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.DTO.Partner
{
    public class PartnerDto
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public string Email { get; set; }

        public List<int> HotelsIds { get; set; } = new List<int>();
    }
}
