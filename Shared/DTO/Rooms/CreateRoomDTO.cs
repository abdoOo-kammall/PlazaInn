using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.DTO.Rooms
{
    public class CreateRoomDTO
    {
       
        public int HotelId { get; set; }
        public string RoomNumber { get; set; }
        public string Type { get; set; }
        public decimal PricePerNight { get; set; }
        public bool IsAvailable { get; set; }
        public List<int> ImageIds { get; set; } = new();

    }
}
