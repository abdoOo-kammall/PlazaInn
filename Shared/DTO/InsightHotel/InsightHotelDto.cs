using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared.DTO.File;

namespace Shared.DTO.InsightHotel
{
    public class InsightHotelDto
    {
        public string? RestaurantDescription { get; set; }
        public string? RestaurantDescriptionAr { get; set; }

        public List<FileDto> RestaurantImageIds { get; set; } = new();
        public List<FileDto> RestaurantMenuPdfIds { get; set; } = new();


        public string? CafeDescription { get; set; }
        public string? CafeDescriptionAr { get; set; }

        public List<FileDto> CafeImageIds { get; set; } = new();
        public List<FileDto> CafeMenuPdfIds { get; set; } = new();


        public Dictionary<string, bool> Facilities { get; set; } = new();
    }
}
