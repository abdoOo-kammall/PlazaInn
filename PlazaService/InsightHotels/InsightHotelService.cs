using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PlazaCore.Entites;
using PlazaCore.RepositoryContract;
using PlazaCore.ServiceContract;

namespace PlazaService.InsightHotels
{
    public class InsightHotelService : IInsightHotelService
    {
        private readonly IGenericRepository<InsightHotel> _insightHotelRepo;

        public InsightHotelService(IGenericRepository<InsightHotel> genericRepository)
        {
            this._insightHotelRepo = genericRepository;
        }
        //public async Task AddInsightHotelAsync(InsightHotel insightHotel)
        //{
        //        await _insightHotelRepo.AddAsync(insightHotel);
        //        await _insightHotelRepo.SaveChangesAsync();
        //}
        public async Task AddInsightHotelAsync(InsightHotel insightHotel)
        {
            try
            {
                await _insightHotelRepo.AddAsync(insightHotel);
                await _insightHotelRepo.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error in AddInsightHotelAsync: {ex.Message}", ex);
            }
        }

        public async Task DeleteInsightHotelAsync(int hotelId)
        {
            var all = await _insightHotelRepo.GetAllAsync();
            var insight = all.FirstOrDefault(i => i.HotelId == hotelId);
            if (insight != null)
            {
                _insightHotelRepo.Delete(insight);
                await _insightHotelRepo.SaveChangesAsync();
            }
        }

        public async Task<InsightHotel?> GetByHotelIdAsync(int hotelId)
        {
            var all = await _insightHotelRepo.GetAllAsync();
            return all.FirstOrDefault(i => i.HotelId == hotelId);
        }

        public async Task UpdateInsightHotelAsync(InsightHotel insightHotel)
        {
            _insightHotelRepo.Update(insightHotel);
            await _insightHotelRepo.SaveChangesAsync();
        }
    }
}
