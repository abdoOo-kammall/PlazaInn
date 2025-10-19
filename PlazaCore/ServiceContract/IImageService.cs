using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using PlazaCore.Entites;

namespace PlazaCore.ServiceContract
{
    public interface IImageService
    {
        Task<List<int>> SaveImagesAsync(List<IFormFile> files , string? entityType );
        Task DeleteImageAsync(int imageId);

    }
}
