using System;

namespace Shared.ImagePathGeneration
{
    public static class ImagePathCreation
    {
        private const string BaseUrl = "http://plazainn.runasp.net/";

        public static string BuildFullImageUrl(string? relativePath)
        {
            if (string.IsNullOrEmpty(relativePath))
                return string.Empty;

           
            var normalizedPath = relativePath.Replace("\\", "/");

            if (normalizedPath.StartsWith("wwwroot/", StringComparison.OrdinalIgnoreCase))
                normalizedPath = normalizedPath.Substring("wwwroot/".Length);

            return $"{BaseUrl}{normalizedPath}";
        }
    }
}
