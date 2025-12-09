using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlazaCore.Entites
{
    public class HomeSlider:BaseEntity
    {
        //public int Id { get; set; }
        public string? Tittle {  get; set; }
        public string? SubTittle { get; set; }
        public string? ButtonText { get; set; }
        public string? ButtonLink { get; set; }


        public string? ImageIds { get; set; }

    }
}
