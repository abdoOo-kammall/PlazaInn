using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlazaCore.Entites
{
    public class Hotel : BaseEntity
    {
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string Address { get; set; } = null!;
        public string City { get; set; } = null!;
        public string Country { get; set; } = null!;
        public double Rating { get; set; }

        public string? ImageIds { get; set; }
        public ICollection<Room> Rooms { get; set; } = new HashSet<Room>();
    }
}
