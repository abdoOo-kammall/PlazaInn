using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared.DTO.File;

namespace Shared.DTO.HomeSlider
{
    public class HomeSliderDto
    {
        public int Id { get; set; }
        public List<FileDto> Images { get; set; }
    }
}
