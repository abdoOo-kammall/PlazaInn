using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared.Enums;

namespace Shared.DTO.Rooms
{
    public class RoomDTO
    {

        public int Id { get; set; }
        public string HotelName { get; set; }
        public string RoomNumber { get; set; }
        public string Type { get; set; }
        public decimal PricePerNight { get; set; }
        public bool IsAvailable { get; set; }

        public List<string> ImageUrls { get; set; } = new List<string>();

    }
}
