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
        //public string RoomNumber { get; set; }
         
        public string? Name { get; set; }
        public string? NameAr { get; set; } 
        public RoomType Type { get; set; }  // Single, Double, Suite
        
        public decimal PricePerNight { get; set; }
        public bool ExtraBed { get; set; } = false;
        public decimal PriceForExtraBed { get; set; } = 0;

        public int MaxSleep { get; set; }
        public string? ImageIds { get; set; }
        public Dictionary<string, bool> Features { get; set; } = new();

        public ICollection<Booking> Bookings { get; set; } = new HashSet<Booking>();



        [ForeignKey("Hotel")]
        public int HotelId { get; set; }
        public Hotel Hotel { get; set; }
    }
}
