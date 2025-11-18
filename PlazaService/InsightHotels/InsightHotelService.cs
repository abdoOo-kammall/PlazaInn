using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using PlazaCore.Entites;
using PlazaCore.RepositoryContract;
using PlazaCore.ServiceContract;
using Shared.Enums;

namespace PlazaService.InsightHotels
{
    public class InsightHotelService : IInsightHotelService
    {
        private readonly IGenericRepository<InsightHotel> _insightHotelRepo;
        private readonly IMenuService _menuService;

        public InsightHotelService(IGenericRepository<InsightHotel> genericRepository , IMenuService menuService)
        {
            this._insightHotelRepo = genericRepository;
            this._menuService = menuService;
        }
        
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

        //public async Task<string> UploadMenuPdfAsync(int hotelId, IFormFile file, MenuType menuType)
        //{
        //    var hotelInsight = await _insightHotelRepo.GetByIdAsync(hotelId);
        //    if (hotelInsight == null) throw new Exception("InsightHotel not found");

        //    if (Path.GetExtension(file.FileName).ToLower() != ".pdf")
        //        throw new Exception("Only PDF allowed");

          
        //    var url = await _menuService.SaveMenuPdfAsync(file, menuType);
        //    var baseUrl = "http://plazainn.runasp.net/";
        //    var fullUrl = $"{baseUrl}{url}";

        //    if (menuType.ToString() == "restaurant")
        //        hotelInsight.RestaurantMenuPdf = fullUrl;
        //    else if (menuType.ToString() == "cafe")
        //        hotelInsight.CafeMenuPdf = fullUrl;
        //    else
        //        throw new Exception("Invalid menu type");

        //    await _insightHotelRepo.SaveChangesAsync();

        //    return fullUrl;
        //}

    }
}
