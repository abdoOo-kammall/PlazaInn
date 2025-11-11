using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlazaCore.Entites
{
    public class Partners : BaseEntity
    {
        //public int Id { get; set; }

        public string Name { get; set; }
        public string Email { get; set; }

        public ICollection<Hotel> Hotels { get; set; } = new HashSet<Hotel>();
    }
}
