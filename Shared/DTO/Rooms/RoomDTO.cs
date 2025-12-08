using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared.DTO.File;
using Shared.Enums;

namespace Shared.DTO.Rooms
{
    public class RoomDTO
    {

        public string Id { get; set; }
        public string? Name { get; set; }
        public string? NameAr { get; set; }
        public string HotelName { get; set; }
        public RoomType Type { get; set; }
        public decimal PricePerNight { get; set; }
        public bool ExtraBed { get; set; } = false;
        public decimal PriceForExtraBed { get; set; } = 0;

        public int MaxSleep { get; set; }
        public Dictionary<string, bool> Features { get; set; } = new();

        public List<FileDto> Images { get; set; } = new List<FileDto>();

    }
}
