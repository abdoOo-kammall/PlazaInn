using Shared.DTO.File;
using Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlazaCore.ServiceContract
{
    public interface ISectionImageService
    {
        Task<IEnumerable<FileDto>> GetImagesByTypeAsync(PlazaInnType type);
        Task UpdateSectionImagesAsync(PlazaInnType type, List<int> newIds);


    }
}
