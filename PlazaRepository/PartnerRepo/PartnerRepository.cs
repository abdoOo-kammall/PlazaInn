using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PlazaCore.Entites;
using PlazaCore.RepositoryContract;
using PlazaCore.Specification.PartnerSpecification;

namespace PlazaRepository.PartnerRepo
{
    public class PartnerRepository : GenericRepository<Partners>, IPartnerRepository
    {
        private readonly PlazaDbContext _db;

        public PartnerRepository(PlazaDbContext plazaDbContext) : base(plazaDbContext) 
        {
            this._db = plazaDbContext;
        }
        public async Task<IEnumerable<Partners>> GetPartnersByHotelIdAsync(int hotelId)
        {
            var spec = new PartnerByHotelIdSpec(hotelId);
            return await GetAllWithSpecAsync(spec);
        }
    }
}
