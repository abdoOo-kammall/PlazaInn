using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.DTO.InsightHotel
{
    public class InsightHotelDto
    {
        public string? RestaurantDescription { get; set; }
        public List<int> RestaurantImageIds { get; set; } = new();

        public string? CafeDescription { get; set; }
        public List<int> CafeImageIds { get; set; } = new();

        public Dictionary<string, bool> Facilities { get; set; } = new();
    }
}
