using System;

namespace Shared.ImagePathGeneration
{
    public static class ImagePathCreation
    {
        private static string _baseUrl = string.Empty;

        /// <summary>Must be called once at startup with the public backend URL (config key "BaseUrl").</summary>
        public static void Configure(string baseUrl)
        {
            if (string.IsNullOrWhiteSpace(baseUrl))
                throw new ArgumentException("BaseUrl must be a non-empty URL.", nameof(baseUrl));

            _baseUrl = baseUrl.TrimEnd('/') + "/";
        }

        public static string BuildFullImageUrl(string? relativePath)
        {
            if (string.IsNullOrEmpty(relativePath))
                return string.Empty;

           
            var normalizedPath = relativePath.Replace("\\", "/").TrimStart('/');

            if (normalizedPath.StartsWith("wwwroot/", StringComparison.OrdinalIgnoreCase))
                normalizedPath = normalizedPath.Substring("wwwroot/".Length);

            return $"{_baseUrl}{normalizedPath}";
        }
    }
}
