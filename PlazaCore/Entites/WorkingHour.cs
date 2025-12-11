using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlazaCore.Entites
{
    public class WorkingHour
    {
        public int Id { get; set; }

        public  DayOfWeek Day { get; set; } 
        public DateTime OpenTime { get; set; }  
        public DateTime CloseTime { get; set; } 
        [ForeignKey("ContactUs")]
        public int ContactUsId { get; set; }
        public ContactUs ContactUs { get; set; } = null!;
    }
}
