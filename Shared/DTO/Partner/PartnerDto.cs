using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.DTO.Partner
{
    public class PartnerDto
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public string Email { get; set; }

        public List<string> HotelsIds { get; set; } = new List<string>();
    }
}
