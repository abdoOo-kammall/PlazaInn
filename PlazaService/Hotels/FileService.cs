using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using PlazaCore.Entites;
using PlazaCore.RepositoryContract;
using PlazaCore.ServiceContract;
using PlazaCore.Specification.SectionImageSpecification;
using Shared.Enums;
using Shared.Helper;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;

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

        public FileService(IWebHostEnvironment env, IGenericRepository<Image> imageRepo, ILogger<FileService> logger)
        {
            _env = env;
            _repo = imageRepo;
            _logger = logger;
        }

        public async Task<List<int>> SaveFilesAsync(List<IFormFile> files, PlazaInnType? type)
        {
            if (type == null || type == PlazaInnType.None || !Enum.IsDefined(typeof(PlazaInnType), type.Value))
                throw new ArgumentException("Invalid file type.");

            var fileType = type.Value;
            var isMenuType = fileType == PlazaInnType.restaurantMenu || fileType == PlazaInnType.cafeMenu;

            // Validate every file up front so a bad file doesn't leave the batch half-saved.
            foreach (var file in files)
                await ValidateFileAsync(file, isMenuType);

            var savedIds = new List<int>();
            var folderName = Methods.GetStorageFolder(fileType.ToString());
            var folder = Path.Combine(GetWebRoot(), folderName, fileType.ToString());
            Directory.CreateDirectory(folder);

            foreach (var file in files)
            {
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

                string fileHash;
                using (var sha256 = SHA256.Create())
                using (var stream = file.OpenReadStream())
                {
                    var hashBytes = await sha256.ComputeHashAsync(stream);
                    fileHash = Convert.ToHexString(hashBytes).ToLowerInvariant();
                }

                var existing = await _repo.GetByIdWithSpecAsync(new ImageByHashSpec(fileHash, fileType));

                if (existing != null)
                {
                    savedIds.Add(existing.Id);
                    continue;
                }

                var tempPath = Path.Combine(folder, $"{Guid.NewGuid():N}.tmp");
                string? finalPath = null;
                Image? image = null;

                try
                {
                    using (var stream = new FileStream(tempPath, FileMode.CreateNew))
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

                    Methods.CacheFilePath(fileType.ToString(), image.Id, image.Url);
                    savedIds.Add(image.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to save uploaded file {FileName} ({Type}).", file.FileName, fileType);
                    await CleanupFailedUploadAsync(tempPath, finalPath, image);
                    throw;
                }
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

            if (!string.IsNullOrEmpty(file.Url))
            {
                var path = Path.Combine(GetWebRoot(), file.Url.Replace("/", Path.DirectorySeparatorChar.ToString()));
                if (File.Exists(path))
                    File.Delete(path);
            }

            _repo.Delete(file);
            await _repo.SaveChangesAsync();
            Methods.InvalidateFilePath(file.Url);
        }

        private string GetWebRoot() => _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");

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
            using (var stream = file.OpenReadStream())
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

        private async Task CleanupFailedUploadAsync(string tempPath, string? finalPath, Image? image)
        {
            TryDeleteFile(tempPath);
            if (finalPath != null)
                TryDeleteFile(finalPath);

            if (image == null)
                return;

            try
            {
                var wasSaved = image.Id > 0;
                _repo.Delete(image);
                if (wasSaved)
                    await _repo.SaveChangesAsync();
            }
            catch (Exception cleanupEx)
            {
                _logger.LogError(cleanupEx, "Failed to remove orphan image record {ImageId}.", image.Id);
            }
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
