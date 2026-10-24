using Plaza.Exceptions;
using PlazaCore.Entites;
using PlazaCore.RepositoryContract;
using PlazaCore.ServiceContract;
using PlazaCore.Specification.InsightHotelSpecification;

namespace PlazaService.InsightHotels
{
    public class InsightHotelService : IInsightHotelService
    {
        private readonly IGenericRepository<InsightHotel> _insightHotelRepo;

        public InsightHotelService(IGenericRepository<InsightHotel> genericRepository)
        {
            this._insightHotelRepo = genericRepository;
        }

        public async Task AddInsightHotelAsync(InsightHotel insightHotel)
        {
            if (await GetByHotelIdAsync(insightHotel.HotelId) != null)
                throw new ConflictException("Insight details already exist for this hotel.");

            await _insightHotelRepo.AddAsync(insightHotel);
            await _insightHotelRepo.SaveChangesAsync();
        }

        public async Task DeleteInsightHotelAsync(int hotelId)
        {
            var insight = await GetByHotelIdAsync(hotelId);
            if (insight != null)
            {
                _insightHotelRepo.Delete(insight);
                await _insightHotelRepo.SaveChangesAsync();
            }
        }

        public async Task<InsightHotel?> GetByHotelIdAsync(int hotelId)
        {
            return await _insightHotelRepo.GetByIdWithSpecAsync(new InsightHotelByHotelIdSpec(hotelId));
        }

        public async Task UpdateInsightHotelAsync(InsightHotel insightHotel)
        {
            _insightHotelRepo.Update(insightHotel);
            await _insightHotelRepo.SaveChangesAsync();
        }
    }
}
