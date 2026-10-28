using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Shared.Enums;
using Shared.Options;

namespace PlazaService.Hotels
{
    internal sealed class CloudinaryFileStorage
    {
        private readonly Cloudinary _client;
        private readonly string _rootFolder;

        public CloudinaryFileStorage(CloudinarySettings settings)
        {
            _client = new Cloudinary(new CloudinaryDotNet.Account(settings.CloudName.Trim(), settings.ApiKey.Trim(), settings.ApiSecret.Trim()));
            _client.Api.Secure = true;
            _rootFolder = string.IsNullOrWhiteSpace(settings.Folder) ? "plaza-inn" : settings.Folder.Trim().Trim('/');
        }

        public async Task<string> UploadAsync(IFormFile file, PlazaInnType type)
        {
            var folder = $"{_rootFolder}/{type}";
            var raw = type == PlazaInnType.restaurantMenu || type == PlazaInnType.cafeMenu;
            var fileName = Path.GetFileName(file.FileName);
            if (string.IsNullOrWhiteSpace(fileName))
                fileName = "upload";

            await using var source = file.OpenReadStream();
            var buffer = new MemoryStream();
            await source.CopyToAsync(buffer);
            buffer.Position = 0;

            using var description = new FileDescription(fileName, buffer);
            UploadResult result;
            if (raw)
            {
                result = await _client.UploadAsync(new RawUploadParams
                {
                    File = description,
                    Folder = folder,
                    UseFilename = false,
                    UniqueFilename = true,
                    Overwrite = false
                }, "raw");
            }
            else
            {
                result = await _client.UploadAsync(new ImageUploadParams
                {
                    File = description,
                    Folder = folder,
                    UseFilename = false,
                    UniqueFilename = true,
                    Overwrite = false
                });
            }

            var secureUrl = result.SecureUrl?.ToString();
            if (result.Error != null || string.IsNullOrWhiteSpace(secureUrl))
                throw new InvalidOperationException(result.Error?.Message ?? "Cloudinary did not return a file URL.");

            return secureUrl;
        }

        public async Task DeleteAsync(string secureUrl)
        {
            if (!TryParsePublicId(secureUrl, out var publicId, out var resourceType))
                throw new InvalidOperationException("Could not read a Cloudinary public id from the stored file URL.");

            var deleted = await _client.DestroyAsync(new DeletionParams(publicId)
            {
                ResourceType = resourceType
            });

            if (deleted.Error != null)
                throw new InvalidOperationException(deleted.Error.Message);
        }

        internal static bool TryParsePublicId(string secureUrl, out string publicId, out ResourceType resourceType)
        {
            publicId = string.Empty;
            resourceType = ResourceType.Image;

            if (!Uri.TryCreate(secureUrl, UriKind.Absolute, out var uri))
                return false;

            var path = uri.AbsolutePath;
            const string uploadMarker = "/upload/";
            var uploadIndex = path.IndexOf(uploadMarker, StringComparison.OrdinalIgnoreCase);
            if (uploadIndex < 0)
                return false;

            var beforeUpload = path[..uploadIndex].Trim('/');
            var resourceSegment = beforeUpload.Split('/', StringSplitOptions.RemoveEmptyEntries).LastOrDefault();
            if (!string.IsNullOrWhiteSpace(resourceSegment))
                resourceType = ParseResourceType(resourceSegment);

            var tail = path[(uploadIndex + uploadMarker.Length)..].Trim('/');
            var parts = tail.Split('/', StringSplitOptions.RemoveEmptyEntries);
            var start = 0;
            while (start < parts.Length && (parts[start].Contains(',') || IsVersion(parts[start])))
                start++;

            if (start >= parts.Length)
                return false;

            publicId = Uri.UnescapeDataString(string.Join('/', parts.Skip(start)));
            if (resourceType != ResourceType.Raw)
            {
                var slash = publicId.LastIndexOf('/');
                var dot = publicId.LastIndexOf('.');
                if (dot > slash)
                    publicId = publicId[..dot];
            }

            return !string.IsNullOrWhiteSpace(publicId);
        }

        private static ResourceType ParseResourceType(string segment) =>
            segment.ToLowerInvariant() switch
            {
                "raw" => ResourceType.Raw,
                "video" => ResourceType.Video,
                "auto" => ResourceType.Auto,
                _ => ResourceType.Image
            };

        private static bool IsVersion(string segment) =>
            segment.Length > 1 &&
            (segment[0] == 'v' || segment[0] == 'V') &&
            segment.Skip(1).All(char.IsDigit);
    }
}
