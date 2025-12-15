using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlazaCore.Entites
{
    public class AboutUs:BaseEntity
    {
        //public int Id { get; set; }
        public string? Tittle {  get; set; }
        public string? SubTittle { get; set; }
        public string? Description { get; set; }

        public string? Square1ImageId { get; set; }
        public string? CircleImageId { get; set; }
        public string? Square2ImageId { get; set; }


    }
}
