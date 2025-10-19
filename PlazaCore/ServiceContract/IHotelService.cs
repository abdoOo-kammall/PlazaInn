using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PlazaCore.Entites;

namespace PlazaCore.ServiceContract
{
    public interface IHotelService
    {
        Task<IEnumerable<Hotel>> GetAllHotelsAsync();
        Task<Hotel?> GetHotelAsync(int id);

        Task AddHotelAsync(Hotel hotel);
        Task UpdateHotelAsync(Hotel hotel);
        Task DeleteHotelAsync(int id);
    }
}
