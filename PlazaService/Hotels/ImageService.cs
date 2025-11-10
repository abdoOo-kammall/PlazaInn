using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using PlazaCore.Entites;
using PlazaCore.RepositoryContract;
using PlazaCore.ServiceContract;
using Shared.Enums;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace PlazaService.Hotels
{
    public class ImageService : IImageService
    {
        private readonly IWebHostEnvironment _env;
        private readonly IGenericRepository<Image> _imageRepo;

        public ImageService(IWebHostEnvironment env, IGenericRepository<Image> imageRepo)
        {
            _env = env;
            _imageRepo = imageRepo;
        }


        //public async Task<List<int>> SaveImagesAsync(List<IFormFile> files, PlazaInnType? entityType)
        //{


        //    var savedIds = new List<int>();
        //    try
        //    {
        //        var root = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");


        //        var folder = Path.Combine(root, "images", entityType.ToString());
        //        Directory.CreateDirectory(folder);

        //        foreach (var file in files)
        //        {
        //            var image = new Image { EntityType = entityType.ToString() };
        //            await _imageRepo.AddAsync(image);
        //            await _imageRepo.SaveChangesAsync();

        //            var fileName = $"{image.Id}{Path.GetExtension(file.FileName)}";
        //            var path = Path.Combine(folder, fileName);

        //            using (var stream = new FileStream(path, FileMode.Create))
        //            {
        //                await file.CopyToAsync(stream);
        //            }

        //            image.Url = Path.Combine("images", entityType.ToString(), fileName).Replace("\\", "/");
        //            await _imageRepo.SaveChangesAsync();

        //            savedIds.Add(image.Id);
        //        }

        //        return savedIds;
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"🔥 ERROR while saving images: {ex}");

        //        throw new Exception($"❌ Error while saving images: {ex.Message}", ex);
        //    }
        //}


        public async Task<List<int>> SaveImagesAsync(List<IFormFile> files, PlazaInnType? entityType)
        {
            var savedIds = new List<int>();
            try
            {
                var root = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
                var folder = Path.Combine(root, "images", entityType.ToString());
                Directory.CreateDirectory(folder);

                foreach (var file in files)
                {
                    // 🧮 احسب الـ hash
                    string fileHash;
                    using (var sha256 = System.Security.Cryptography.SHA256.Create())
                    using (var stream = file.OpenReadStream())
                    {
                        var hashBytes = sha256.ComputeHash(stream);
                        fileHash = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
                    }

                    // 🔍 شيّك لو في صورة بنفس الـ hash بالفعل
                    var existingImage = (await _imageRepo.GetAllAsync())
                        .FirstOrDefault(i => i.Url != null && i.EntityType == entityType.ToString() && i.Hash == fileHash);

                    if (existingImage != null)
                    {
                        savedIds.Add(existingImage.Id);
                        continue; // متكملش في الحفظ، الصورة دي موجودة بالفعل
                    }

                    // ✳️ لو جديدة
                    var image = new Image
                    {
                        EntityType = entityType.ToString(),
                        Hash = fileHash
                    };

                    await _imageRepo.AddAsync(image);
                    await _imageRepo.SaveChangesAsync();

                    var fileName = $"{image.Id}{Path.GetExtension(file.FileName)}";
                    var path = Path.Combine(folder, fileName);

                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    image.Url = Path.Combine("images", entityType.ToString(), fileName).Replace("\\", "/");
                    await _imageRepo.SaveChangesAsync();

                    savedIds.Add(image.Id);
                }

                return savedIds;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"🔥 ERROR while saving images: {ex}");
                throw new Exception($"❌ Error while saving images: {ex.Message}", ex);
            }
        }

        public async Task<Image?> GetImageByIdAsync(int id)
        {
            return await _imageRepo.GetByIdAsync(id);
        }

        public async Task DeleteImageAsync(int imageId)
        {
            var image = await _imageRepo.GetByIdAsync(imageId);
            if (image == null) return;

            var path = Path.Combine(_env.WebRootPath, image.Url.Replace("/", Path.DirectorySeparatorChar.ToString()));
            if (File.Exists(path))
                File.Delete(path);

            _imageRepo.Delete(image);
            await _imageRepo.SaveChangesAsync();
        }
    }
}
