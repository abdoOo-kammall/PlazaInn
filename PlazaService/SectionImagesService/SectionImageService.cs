using PlazaCore.Entites;
using PlazaCore.RepositoryContract;
using PlazaCore.ServiceContract;
using PlazaCore.Specification.SectionImageSpecification;
using Shared.DTO.File;
using Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlazaService.SectionImagesService
{
    public class SectionImageService : ISectionImageService
    {
     
      
            private readonly IGenericRepository<Image> _repo;

            public SectionImageService(IGenericRepository<Image> repo)
            {
                _repo = repo;
            }

            public async Task<IEnumerable<FileDto>> GetImagesByTypeAsync(PlazaInnType type)
            {
                var spec = new ImagesByTypeSpecification(type);
                var images = await _repo.GetAllWithSpecAsync(spec);

                return images.Select(i => new FileDto
                {
                    Id = i.Id,
                    Url = i.Url
                });
            }

            public async Task UpdateSectionImagesAsync(PlazaInnType type, List<int> newIds)
            {
                // 1) هات الصور القديمة
                var oldSpec = new ImagesByTypeSpecification(type);
                var oldImages = await _repo.GetAllWithSpecAsync(oldSpec);

                // فك الربط القديم
                foreach (var img in oldImages)
                {
                    img.Type = PlazaInnType.None;
                    _repo.Update(img);
                }

                // 2) هات الصور الجديدة
                var newSpec = new ImagesByIdsSpecification(newIds);
                var newImages = await _repo.GetAllWithSpecAsync(newSpec);

                foreach (var img in newImages)
                {
                    img.Type = type;
                    _repo.Update(img);
                }

                await _repo.SaveChangesAsync();
            }
        

    }
}
