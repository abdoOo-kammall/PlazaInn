using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Shared.DTO.File;
using Shared.ImagePathGeneration;

namespace Shared.Helper
{
    public static class Methods
    {
           // Key: "{storageFolder}/{entityType}/{id}", value: path relative to the web root, an absolute URL, or null when no file exists.
           private static readonly ConcurrentDictionary<string, string?> FilePathCache = new(StringComparer.OrdinalIgnoreCase);

           // Image.Id -> Image.Url (wwwroot relative path or absolute Cloudinary URL).
           private static readonly ConcurrentDictionary<int, string?> StoredLocations = new();

           private static string _webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

           public static MemoryStream CloneStream(MemoryStream original)
           {
                var newStream = new MemoryStream(original.ToArray());
                newStream.Position = 0;
                return newStream;
           }

           /// <summary>Points file lookups at the folder served as static files. Call once at startup.</summary>
           public static void ConfigureFileStorage(string webRootPath)
           {
                if (string.IsNullOrWhiteSpace(webRootPath))
                    throw new ArgumentException("Web root path must be provided.", nameof(webRootPath));

                _webRootPath = webRootPath;
                FilePathCache.Clear();
                StoredLocations.Clear();
           }

           public static bool IsRemoteUrl(string? value)
           {
                if (string.IsNullOrWhiteSpace(value))
                    return false;

                var trimmed = value.Trim();
                return trimmed.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
                    || trimmed.StartsWith("http://", StringComparison.OrdinalIgnoreCase);
           }

           /// <summary>Records Image.Url so mapping can return a Cloudinary URL without scanning wwwroot.</summary>
           public static void RememberStoredLocation(int id, string? storedLocation)
           {
                if (id <= 0)
                    return;

                StoredLocations[id] = string.IsNullOrWhiteSpace(storedLocation) ? null : storedLocation.Trim();
           }

           public static void ForgetStoredLocation(int id, string? entityType)
           {
                if (id <= 0)
                    return;

                StoredLocations.TryRemove(id, out _);
                if (string.IsNullOrWhiteSpace(entityType))
                    return;

                FilePathCache[BuildCacheKey(GetStorageFolder(entityType), entityType, id)] = null;
           }

           public static string GetStorageFolder(string entityType) =>
                entityType.Contains("menu", StringComparison.OrdinalIgnoreCase) ? "menus" : "images";

           /// <summary>Records where an uploaded file was stored so mapping never has to scan the disk for it.</summary>
           public static void CacheFilePath(string entityType, int id, string? relativePath) =>
                FilePathCache[BuildCacheKey(GetStorageFolder(entityType), entityType, id)] = relativePath;

           /// <summary>Marks a deleted file (given by its stored relative URL, e.g. "images/hotel/5.jpg") as missing.</summary>
           public static void InvalidateFilePath(string? relativePath)
           {
                if (string.IsNullOrWhiteSpace(relativePath))
                    return;

                var normalized = relativePath.Replace('\\', '/').Trim('/');
                var separator = normalized.LastIndexOf('/');
                if (separator <= 0 || !int.TryParse(Path.GetFileNameWithoutExtension(normalized), out var id))
                    return;

                FilePathCache[$"{normalized[..separator]}/{id}"] = null;
           }

           public static List<FileDto> ConvertIdsToFileDTO(string? fileIdsJson, string entityType = "general")
           {
                if (string.IsNullOrEmpty(fileIdsJson))
                    return new List<FileDto>();

                List<int>? ids;
                try
                {
                    ids = JsonSerializer.Deserialize<List<int>>(fileIdsJson);
                }
                catch (JsonException)
                {
                    return new List<FileDto>();
                }

                if (ids == null || ids.Count == 0)
                    return new List<FileDto>();

                var baseFolder = GetStorageFolder(entityType);
                var list = new List<FileDto>(ids.Count);

                foreach (var id in ids)
                {
                    if (StoredLocations.TryGetValue(id, out var stored) && IsRemoteUrl(stored))
                    {
                        list.Add(new FileDto
                        {
                            Id = id,
                            Url = stored!.Trim()
                        });
                        continue;
                    }

                    var relativePath = ResolveRelativePath(baseFolder, entityType, id);
                    if (relativePath != null)
                    {
                        list.Add(new FileDto
                        {
                            Id = id,
                            Url = IsRemoteUrl(relativePath)
                                ? relativePath.Trim()
                                : ImagePathCreation.BuildFullImageUrl(relativePath)
                        });
                    }
                }

                return list;
           }

           private static string BuildCacheKey(string baseFolder, string entityType, int id) => $"{baseFolder}/{entityType}/{id}";

           private static string? ResolveRelativePath(string baseFolder, string entityType, int id)
           {
                var key = BuildCacheKey(baseFolder, entityType, id);
                if (FilePathCache.TryGetValue(key, out var cached))
                    return cached;

                string? relativePath;
                try
                {
                    relativePath = FindFileOnDisk(baseFolder, entityType, id);
                }
                catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
                {
                    // Not cached, so a transient I/O failure is retried on the next lookup.
                    return null;
                }

                return FilePathCache.GetOrAdd(key, relativePath);
           }

           private static string? FindFileOnDisk(string baseFolder, string entityType, int id)
           {
                var folderPath = Path.Combine(_webRootPath, baseFolder, entityType);
                if (!Directory.Exists(folderPath))
                    return null;

                var matchedFile = Directory.EnumerateFiles(folderPath, $"{id}.*").FirstOrDefault();
                return matchedFile == null
                    ? null
                    : Path.GetRelativePath(_webRootPath, matchedFile).Replace('\\', '/');
           }
    }
}
