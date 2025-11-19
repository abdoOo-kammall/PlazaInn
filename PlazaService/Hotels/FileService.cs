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
    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _env;
        private readonly IGenericRepository<Image> _repo;

        public FileService(IWebHostEnvironment env, IGenericRepository<Image> imageRepo)
        {
            _env = env;
            _repo = imageRepo;
        }


   

        public async Task<List<int>> SaveFilesAsync(List<IFormFile> files, PlazaInnType? type)
        {
            //var savedIds = new List<int>();
            try
            {
                #region old code 
                //var root = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
                //var folder = Path.Combine(root, "images", entityType.ToString());
                //Directory.CreateDirectory(folder);

                //foreach (var file in files)
                //{
                //    // 🧮 احسب الـ hash
                //    string fileHash;
                //    using (var sha256 = System.Security.Cryptography.SHA256.Create())
                //    using (var stream = file.OpenReadStream())
                //    {
                //        var hashBytes = sha256.ComputeHash(stream);
                //        fileHash = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
                //    }

                //    // 🔍 شيّك لو في صورة بنفس الـ hash بالفعل
                //    var existingImage = (await _imageRepo.GetAllAsync())
                //        .FirstOrDefault(i => i.Url != null && i.EntityType == entityType.ToString() && i.Hash == fileHash);

                //    if (existingImage != null)
                //    {
                //        savedIds.Add(existingImage.Id);
                //        continue; // متكملش في الحفظ، الصورة دي موجودة بالفعل
                //    }

                //    // ✳️ لو جديدة
                //    var image = new Image
                //    {
                //        EntityType = entityType.ToString(),
                //        Hash = fileHash
                //    };

                //    await _imageRepo.AddAsync(image);
                //    await _imageRepo.SaveChangesAsync();

                //    var fileName = $"{image.Id}{Path.GetExtension(file.FileName)}";
                //    var path = Path.Combine(folder, fileName);

                //    using (var stream = new FileStream(path, FileMode.Create))
                //    {
                //        await file.CopyToAsync(stream);
                //    }

                //    image.Url = Path.Combine("images", entityType.ToString(), fileName).Replace("\\", "/");
                //    await _imageRepo.SaveChangesAsync();

                //    savedIds.Add(image.Id);
                //}

                //return savedIds;
                #endregion

                var savedIds = new List<int>();
                //var folderName = type.ToString().ToLower().Contains("pdf") ? "menus" : "images";
                var folderName = type.ToString().ToLower().Contains("menu") ? "menus" : "images";

                var root = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
                var folder = Path.Combine(root, folderName, type.ToString());
                Directory.CreateDirectory(folder);

                foreach (var file in files)
                {
                    var isPdfType = type == PlazaInnType.restaurantMenu || type == PlazaInnType.cafeMenu;

                    if (isPdfType && Path.GetExtension(file.FileName).ToLower() != ".pdf")
                        throw new Exception("Only PDF allowed");


                    string fileHash;
                    using (var sha256 = SHA256.Create())
                    using (var stream = file.OpenReadStream())
                    {
                        var hashBytes = sha256.ComputeHash(stream);
                        fileHash = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
                    }

                    var existing = (await _repo.GetAllAsync())
                        .FirstOrDefault(x => x.Hash == fileHash && x.Type == type);

                    if (existing != null)
                    {
                        savedIds.Add(existing.Id);
                        continue;
                    }

                    var image = new Image
                    {
                        Type =(PlazaInnType) type,
                        Hash = fileHash
                    };

                    await _repo.AddAsync(image);
                    await _repo.SaveChangesAsync();

                    var fileName = $"{image.Id}{Path.GetExtension(file.FileName)}";
                    var path = Path.Combine(folder, fileName);

                    using (var stream = new FileStream(path, FileMode.Create))
                        await file.CopyToAsync(stream);

                    image.Url = Path.Combine(folderName, type.ToString(), fileName).Replace("\\", "/");
                    await _repo.SaveChangesAsync();

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

        public async Task<Image?> GetFileByIdAsync(int id)
        {
            return await _repo.GetByIdAsync(id);
        }

        public async Task DeleteFileAsync(int Id)
        {
            var file = await _repo.GetByIdAsync(Id);
            if (file == null) return;

            var path = Path.Combine(_env.WebRootPath, file.Url.Replace("/", Path.DirectorySeparatorChar.ToString()));
            if (File.Exists(path))
                File.Delete(path);

            _repo.Delete(file);
            await _repo.SaveChangesAsync();
        }
    }
}
