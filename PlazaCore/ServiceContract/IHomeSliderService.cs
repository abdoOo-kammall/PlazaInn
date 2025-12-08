using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PlazaCore.Entites;
using Shared.DTO.HomeSlider;

namespace PlazaCore.ServiceContract
{
    public interface IHomeSliderService
    {
        Task<IEnumerable<HomeSliderDto>> GetAllHomeSliderAsync();
        Task<HomeSliderDto?> GetHomeSliderAsync(int id);

        Task AddHomeSliderAsync(CreateHomeSliderDto homeSlider);
        Task UpdateHomeSliderAsync(int id , UpdateHomeSliderDto homeSliderDto);
        Task DeleteHomeSliderAsync(int id);
    }
}
