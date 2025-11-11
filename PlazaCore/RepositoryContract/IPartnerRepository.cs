using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PlazaCore.Entites;

namespace PlazaCore.RepositoryContract
{
    public interface IPartnerRepository :IGenericRepository<Partners>
    {
        Task<IEnumerable<Partners>> GetPartnersByHotelIdAsync(int hotelId);

    }
}
