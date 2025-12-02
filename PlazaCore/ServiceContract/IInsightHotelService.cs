using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using PlazaCore.Entites;
using Shared.Enums;

namespace PlazaCore.ServiceContract
{
    public interface IInsightHotelService
    {
        Task AddInsightHotelAsync(InsightHotel insightHotel);
        Task<InsightHotel?> GetByHotelIdAsync(int hotelId);
        Task UpdateInsightHotelAsync(InsightHotel insightHotel);
        Task DeleteInsightHotelAsync(int hotelId);


    }
}
