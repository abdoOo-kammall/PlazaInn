using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared.Enums;

namespace PlazaCore.Entites
{
    public class Image : BaseEntity
    {
        public string? Url { get; set; } 
        //public bool IsMain { get; set; }
        public PlazaInnType Type { get; set; }  
        public string Hash { get; set; }

    }
}
