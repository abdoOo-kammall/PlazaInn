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
    public interface IFileService
    {
        Task<List<int>> SaveFilesAsync(List<IFormFile> files , PlazaInnType? entityType );
        Task DeleteFileAsync(int Id);
        Task<Image?> GetFileByIdAsync(int id);


    }
}
