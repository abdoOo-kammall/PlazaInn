using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.DTO.HomeSlider
{
    public class CreateHomeSliderDto
    {

        public string? Tittle { get; set; }
        public string? SubTittle { get; set; }
        public string? ButtonText { get; set; }
        public string? ButtonLink { get; set; }


        public List<int> ImagesId {  get; set; }
    }
}
