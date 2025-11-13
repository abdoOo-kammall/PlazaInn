using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.DTO.InsightHotel
{
    public class CreateInsightHotelDto
    {
        public string EncodedHotelId { get; set; } = null!;
        public string? RestaurantDescription { get; set; }
        public string? RestaurantDescriptionAr { get; set; }
        public List<int>? RestaurantImageIds { get; set; }
        public string? CafeDescription { get; set; }
        public string? CafeDescriptionAr { get; set; }
        public List<int>? CafeImageIds { get; set; }
        public Dictionary<string, bool>? Facilities { get; set; }
    }
}
