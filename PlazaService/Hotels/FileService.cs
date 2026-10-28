using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PlazaCore.Entites;
using PlazaCore.RepositoryContract;
using PlazaCore.ServiceContract;
using PlazaCore.Specification.SectionImageSpecification;
using Shared.Enums;
using Shared.Helper;
using Shared.Options;
using System.Security.Cryptography;

namespace PlazaService.Hotels
{
    public class FileService : IFileService
    {
        private const long MaxFileSizeBytes = 10L * 1024 * 1024;

        private static readonly HashSet<string> ImageExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg", ".jpeg", ".png", ".webp"
        };

        private static readonly HashSet<string> MenuExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".pdf"
        };

        private readonly IWebHostEnvironment _env;
        private readonly IGenericRepository<Image> _repo;
        private readonly ILogger<FileService> _logger;
        private readonly CloudinaryFileStorage? _cloud;

        public FileService(
            IWebHostEnvironment env,
            IGenericRepository<Image> imageRepo,
            ILogger<FileService> logger,
            IOptions<CloudinarySettings> cloudinaryOptions)
        {
            _env = env;
            _repo = imageRepo;
            _logger = logger;
            var settings = cloudinaryOptions.Value;
            _cloud = settings.IsConfigured ? new CloudinaryFileStorage(settings) : null;
        }

        public async Task<List<int>> SaveFilesAsync(List<IFormFile> files, PlazaInnType? type)
        {
            if (type == null || type == PlazaInnType.None || !Enum.IsDefined(typeof(PlazaInnType), type.Value))
                throw new ArgumentException("Invalid file type.");

            var fileType = type.Value;
            var isMenuType = fileType == PlazaInnType.restaurantMenu || fileType == PlazaInnType.cafeMenu;

            foreach (var file in files)
                await ValidateFileAsync(file, isMenuType);

            var savedIds = new List<int>();
            var folderName = Methods.GetStorageFolder(fileType.ToString());
            string? localFolder = null;
            if (_cloud == null)
            {
                localFolder = Path.Combine(GetWebRoot(), folderName, fileType.ToString());
                Directory.CreateDirectory(localFolder);
            }

            foreach (var file in files)
            {
                var fileHash = await ComputeHashAsync(file);
                var existing = await _repo.GetByIdWithSpecAsync(new ImageByHashSpec(fileHash, fileType));
                if (existing != null)
                {
                    Remember(existing.Id, existing.Url, fileType);
                    savedIds.Add(existing.Id);
                    continue;
                }

                var id = _cloud == null
                    ? await SaveToWwwRootAsync(file, fileType, fileHash, folderName, localFolder!)
                    : await SaveToCloudinaryAsync(file, fileType, fileHash);
                savedIds.Add(id);
            }

            return savedIds;
        }

        public async Task<Image?> GetFileByIdAsync(int id)
        {
            return await _repo.GetByIdAsync(id);
        }

        public async Task DeleteFileAsync(int Id)
        {
            var file = await _repo.GetByIdAsync(Id);
            if (file == null) return;

            if (Methods.IsRemoteUrl(file.Url))
            {
                if (_cloud == null)
                {
                    _logger.LogWarning(
                        "Image {ImageId} is stored at a remote URL, but Cloudinary is not configured. The database row will be removed and the remote file will be left in place.",
                        file.Id);
                }
                else
                {
                    try
                    {
                        await _cloud.DeleteAsync(file.Url!);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Cloudinary file for image {ImageId} could not be deleted. The database row will still be removed.", file.Id);
                    }
                }
            }
            else if (!string.IsNullOrEmpty(file.Url))
            {
                var path = Path.Combine(GetWebRoot(), file.Url.Replace("/", Path.DirectorySeparatorChar.ToString()));
                if (File.Exists(path))
                    File.Delete(path);
            }

            _repo.Delete(file);
            await _repo.SaveChangesAsync();
            Methods.ForgetStoredLocation(file.Id, file.Type.ToString());
            Methods.InvalidateFilePath(file.Url);
        }

        private async Task<int> SaveToCloudinaryAsync(IFormFile file, PlazaInnType fileType, string fileHash)
        {
            var secureUrl = await _cloud!.UploadAsync(file, fileType);
            Image? image = null;
            try
            {
                image = new Image
                {
                    Type = fileType,
                    Hash = fileHash,
                    Url = secureUrl
                };

                await _repo.AddAsync(image);
                await _repo.SaveChangesAsync();
                Remember(image.Id, secureUrl, fileType);
                return image.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save uploaded file {FileName} ({Type}).", file.FileName, fileType);
                try
                {
                    await _cloud.DeleteAsync(secureUrl);
                }
                catch (Exception cleanupEx)
                {
                    _logger.LogWarning(cleanupEx, "Failed to roll back Cloudinary upload for {FileName}.", file.FileName);
                }

                await RemoveImageRowAsync(image);
                throw;
            }
        }

        private async Task<int> SaveToWwwRootAsync(IFormFile file, PlazaInnType fileType, string fileHash, string folderName, string folder)
        {
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var tempPath = Path.Combine(folder, $"{Guid.NewGuid():N}.tmp");
            string? finalPath = null;
            Image? image = null;

            try
            {
                await using (var stream = new FileStream(tempPath, FileMode.CreateNew))
                    await file.CopyToAsync(stream);

                image = new Image
                {
                    Type = fileType,
                    Hash = fileHash
                };

                await _repo.AddAsync(image);
                await _repo.SaveChangesAsync();

                var fileName = $"{image.Id}{extension}";
                finalPath = Path.Combine(folder, fileName);
                File.Move(tempPath, finalPath, overwrite: true);

                image.Url = $"{folderName}/{fileType}/{fileName}";
                await _repo.SaveChangesAsync();

                Remember(image.Id, image.Url, fileType);
                return image.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save uploaded file {FileName} ({Type}).", file.FileName, fileType);
                await CleanupFailedUploadAsync(tempPath, finalPath, image);
                throw;
            }
        }

        private static void Remember(int id, string? storedUrl, PlazaInnType type)
        {
            Methods.RememberStoredLocation(id, storedUrl);
            if (!string.IsNullOrEmpty(storedUrl))
                Methods.CacheFilePath(type.ToString(), id, storedUrl);
        }

        private string GetWebRoot() => _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");

        private static async Task<string> ComputeHashAsync(IFormFile file)
        {
            using var sha256 = SHA256.Create();
            await using var stream = file.OpenReadStream();
            var hashBytes = await sha256.ComputeHashAsync(stream);
            return Convert.ToHexString(hashBytes).ToLowerInvariant();
        }

        private static async Task ValidateFileAsync(IFormFile file, bool isMenuType)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("One of the uploaded files is empty.");

            if (file.Length > MaxFileSizeBytes)
                throw new ArgumentException($"File '{file.FileName}' exceeds the maximum allowed size of {MaxFileSizeBytes / (1024 * 1024)} MB.");

            var extension = Path.GetExtension(file.FileName);
            var allowed = isMenuType ? MenuExtensions : ImageExtensions;
            if (string.IsNullOrEmpty(extension) || !allowed.Contains(extension))
                throw new ArgumentException($"File '{file.FileName}' has an unsupported extension. Allowed: {string.Join(", ", allowed)}.");

            var header = new byte[12];
            int read;
            await using (var stream = file.OpenReadStream())
                read = await stream.ReadAtLeastAsync(header, header.Length, throwOnEndOfStream: false);

            if (!HasValidSignature(header.AsSpan(0, read), extension.ToLowerInvariant()))
                throw new ArgumentException($"File '{file.FileName}' content does not match its extension.");
        }

        private static bool HasValidSignature(ReadOnlySpan<byte> header, string extension)
        {
            return extension switch
            {
                ".jpg" or ".jpeg" => header.StartsWith(new byte[] { 0xFF, 0xD8, 0xFF }),
                ".png" => header.StartsWith(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }),
                ".webp" => header.Length >= 12
                           && header[..4].SequenceEqual("RIFF"u8)
                           && header.Slice(8, 4).SequenceEqual("WEBP"u8),
                ".pdf" => header.StartsWith("%PDF"u8),
                _ => false
            };
        }

        private async Task RemoveImageRowAsync(Image? image)
        {
            if (image == null || image.Id <= 0)
                return;

            try
            {
                _repo.Delete(image);
                await _repo.SaveChangesAsync();
            }
            catch (Exception cleanupEx)
            {
                _logger.LogError(cleanupEx, "Failed to remove orphan image record {ImageId}.", image.Id);
            }
        }

        private async Task CleanupFailedUploadAsync(string tempPath, string? finalPath, Image? image)
        {
            TryDeleteFile(tempPath);
            if (finalPath != null)
                TryDeleteFile(finalPath);

            await RemoveImageRowAsync(image);
        }

        private void TryDeleteFile(string path)
        {
            try
            {
                if (File.Exists(path))
                    File.Delete(path);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to delete temporary upload file {Path}.", path);
            }
        }
    }
}
