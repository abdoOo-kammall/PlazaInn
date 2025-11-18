using System;
using System.Collections.Generic;
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
        public static List<FileDto> ConvertIdsToFileDTO(string fileIdsJson, string entityType = "general")
        {
            if (string.IsNullOrEmpty(fileIdsJson))
                return new List<FileDto>();

            try
            {
                var ids = JsonSerializer.Deserialize<List<int>>(fileIdsJson);
                if (ids == null || !ids.Any())
                    return new List<FileDto>();

                var list = new List<FileDto>();
                var baseFolder = entityType.ToLower().Contains("menu") ? "menus" : "images";
                var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", baseFolder, entityType);


                foreach (var id in ids)
                {
                    var matchedFile = Directory.GetFiles(folderPath, $"{id}.*").FirstOrDefault();
                    if (matchedFile != null)
                    {
                        var relativePath = matchedFile.Replace(Directory.GetCurrentDirectory(), "")
                                                      .Replace("\\", "/")
                                                      .TrimStart('/');
                        list.Add(new FileDto
                        {
                            Id = id,
                            Url = ImagePathCreation.BuildFullImageUrl(relativePath)
                        });
                    }
                }

                return list;
            }
            catch
            {
                return new List<FileDto>();
            }
        }
    }
}
