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
        Task<IEnumerable<Hotel>> GetAllHotelsAsync(decimal? minPrice = null, decimal? maxPrice = null);
        Task<Hotel?> GetHotelAsync(int id);
        Task<IReadOnlyList<Hotel>> GetHotelsByIdsAsync(IReadOnlyCollection<int> ids);

        Task AddHotelAsync(Hotel hotel);
        Task UpdateHotelAsync(Hotel hotel);
        Task DeleteHotelAsync(int id);
    }
}
