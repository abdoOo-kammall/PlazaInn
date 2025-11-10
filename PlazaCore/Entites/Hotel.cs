using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlazaCore.Entites
{
    public class Hotel : BaseEntity
    {
        public string Name { get; set; } = null!;
        public string NameAr { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string DescriptionAr { get; set; } = null!;

        public string Address { get; set; } = null!;
        public string AddressAr { get; set; }
        public string City { get; set; } = null!;
        public string CityAr { get; set; }
        public string Location { get; set; } = null!;
        public string Phone {  get; set; }

        //public string Email { get; set; } = null!;
        public string Email { get; set; } = null!;

        public int NumOfAvailableRoomsToReserve { get; set; }
        public int NumOfAvailableSuitesToReserve { get; set; }
        public double Rating { get; set; }
        // socail media 
        public string? Instagram {  get; set; }
        public string? Facebook { get; set; }
        public string? WhatsApp { get; set; }
        public string? Space { get; set; }
        
        public string? ImageIds { get; set; }
        public ICollection<Room> Rooms { get; set; } = new HashSet<Room>();
        public InsightHotel? InsightHotel { get; set; }
    }
}
