using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PlazaCore.ServiceContract;
using PlazaCore.Entites;
using PlazaCore.RepositoryContract;
using Microsoft.EntityFrameworkCore;
using PlazaCore.Specification.PartnerSpecification;

namespace PlazaService.Partner
{
    public class PartnerService : IPartnerService
    {
        private readonly IPartnerRepository _partnerRepo;

        public PartnerService(IPartnerRepository partnerRepository)
        {
            this._partnerRepo = partnerRepository;
        }
        public async Task AddPartnerAsync(Partners partner)
        {
            await _partnerRepo.AddPartnerWithHotelsAsync(partner);
            await _partnerRepo.SaveChangesAsync();  
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
            var spec = new PartnersWithHotelsSpec();
            return await _partnerRepo.GetAllWithSpecAsync(spec);
        }

        public async Task<Partners?> GetPartnerAsync(int id)
        {
            var spec = new PartnerByIdWithHotelsSpec(id);
            //return await _partnerRepo.GetByIdAsync(id);
            return await _partnerRepo.GetByIdWithSpecAsync(spec);
        }

        public async Task<IEnumerable<Partners>> GetPartnersByHotelAsync(int hotelId)
        {
            
            return await _partnerRepo.GetPartnersByHotelIdAsync(hotelId);
        }

        public async Task UpdatePartnerAsync(Partners partner)
        {
            _partnerRepo.UpdatePartnerWithHotelsAsync(partner);
            await _partnerRepo.SaveChangesAsync();
        }
    }
}
    