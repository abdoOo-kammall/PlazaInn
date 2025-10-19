using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared.Enums;

namespace PlazaCore.Entites
{
    public class Room : BaseEntity
    {
        public string RoomNumber { get; set; } 
        public RoomType Type { get; set; }  // Single, Double, Suite
        public decimal PricePerNight { get; set; }
        public bool IsAvailable { get; set; }

        public string? ImageIds { get; set; }




        [ForeignKey("Hotel")]
        public int HotelId { get; set; }
        public Hotel Hotel { get; set; }
    }
}
