using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.DTO.AboutUs
{
    public class CreateAboutUsDto
    {
        public string? Tittle { get; set; }
        public string? SubTittle { get; set; }
        public string? Description { get; set; }
        public List<int> ImagesId { get; set; }
    }
}
