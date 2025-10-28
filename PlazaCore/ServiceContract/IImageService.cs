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
    public interface IImageService
    {
        Task<List<int>> SaveImagesAsync(List<IFormFile> files , PlazaInnType? entityType );
        Task DeleteImageAsync(int imageId);
        Task<Image?> GetImageByIdAsync(int id);


    }
}
