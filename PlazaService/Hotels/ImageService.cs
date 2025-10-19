using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using PlazaCore.Entites;
using PlazaCore.RepositoryContract;
using PlazaCore.ServiceContract;

namespace PlazaService.Hotels
{
    public class ImageService : IImageService
    {
        private readonly IWebHostEnvironment _env;
        private readonly IGenericRepository<Image> _imageRepo;

        public ImageService(IWebHostEnvironment env , IGenericRepository<Image> genericRepository)
        {
            _env = env;
            this._imageRepo = genericRepository;
        }

        public async Task<List<int>> SaveImagesAsync(List<IFormFile> files, string? entityType = null)
        {
            var savedIds = new List<int>();
            var folder = Path.Combine(_env.WebRootPath, "images", entityType ?? "general");
            Directory.CreateDirectory(folder);

            foreach (var file in files)
            {
                // أولًا ننشئ Image بدون URL
                var image = new Image { EntityType = entityType };
                await _imageRepo.AddAsync(image);
                await _imageRepo.SaveChangesAsync(); // علشان ناخد Id

                // نحفظ الملف باسم Id
                var fileName = $"{image.Id}{Path.GetExtension(file.FileName)}";
                var path = Path.Combine(folder, fileName);

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // نحدث URL بعد كده
                image.Url = $"/images/{entityType}/{fileName}";
                await _imageRepo.SaveChangesAsync();

                savedIds.Add(image.Id);
            }

            return savedIds;
        }


      

        public async Task DeleteImageAsync(int imageId)
        {
            var image = await _imageRepo.GetByIdAsync(imageId);
            if (image == null) return;

            var fileName = Path.GetFileName(image.Url);
            var entityFolder = image.EntityType ?? "unknown";

            var path = Path.Combine(_env.WebRootPath, "images", entityFolder, fileName);

            if (File.Exists(path))
                File.Delete(path);

            _imageRepo.Delete(image);
            await _imageRepo.SaveChangesAsync();
        }
    }
}
