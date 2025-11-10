using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlazaCore.Entites
{
    public class Image : BaseEntity
    {
        public string? Url { get; set; } 
        //public bool IsMain { get; set; }
        public string EntityType { get; set; }  // "Hotel" أو "Room"
        public string Hash { get; set; }

    }
}
