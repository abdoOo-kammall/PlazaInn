using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PlazaCore.ServiceContract;
using PlazaCore.Entites;
using PlazaCore.RepositoryContract;

namespace PlazaService.Partner
{
    public class PartnerService : IPartnerService
    {
        private readonly IPartnerRepository _partnerRepo;

        public PartnerService(IPartnerRepository partnerRepository)
        {
            this._partnerRepo = partnerRepository;
        }
        public Task AddPartnerAsync(Partners room)
        {
            throw new NotImplementedException();
        }

        public async Task DeletePartnerAsync(int id)
        {
         var partner =   await _partnerRepo.GetByIdAsync(id);
            if (partner != null) {
                     _partnerRepo.Delete(partner);
                await _partnerRepo.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Partners>> GetAllPartnersAsync()
        {
            return await _partnerRepo.GetAllAsync();
        }

        public async Task<Partners?> GetPartnerAsync(int id)
        {
            return await _partnerRepo.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Partners>> GetPartnersByHotelAsync(int hotelId)
        {
            return await _partnerRepo.GetPartnersByHotelIdAsync(hotelId);
        }

        public async Task UpdatePartnerAsync(Partners partner)
        {
            _partnerRepo.Update(partner);
            await _partnerRepo.SaveChangesAsync();
        }
    }
}
    