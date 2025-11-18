using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlazaCore.Entites
{
    public class InsightHotel : BaseEntity
    {
        public string? RestaurantDescription { get; set; }
        public string? RestaurantDescriptionAr { get; set; }
        public string? CafeDescription { get; set; }
        public string? CafeDescriptionAr { get; set; }

        public string? RestaurantImageIds { get; set; }
        public string? CafeImageIds { get; set; }
        public string? RestaurantMenuPdfIds { get; set; }
        public string? CafeMenuPdfIds { get; set; }

        public Dictionary<string, bool> Facilities { get; set; }

        [ForeignKey("Hotel")]
        public int HotelId { get; set; }
        public Hotel Hotel { get; set; } = null!;
    }
    }
