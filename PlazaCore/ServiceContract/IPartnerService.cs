using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PlazaCore.Entites;
using Shared.DTO.Partner;

namespace PlazaCore.ServiceContract
{
    public interface IPartnerService
    {
        Task<IEnumerable<Partners>> GetAllPartnersAsync();
        Task<Partners?> GetPartnerAsync(int id);
        Task AddPartnerAsync(Partners partner);
        Task UpdatePartnerAsync(Partners partner);
        Task DeletePartnerAsync(int id);
        Task<IEnumerable<Partners>> GetPartnersByHotelAsync(int hotelId);

    }
}
