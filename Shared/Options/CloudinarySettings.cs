namespace Shared.Options
{
    public class CloudinarySettings
    {
        public const string SectionName = "CloudinarySettings";

        public const string PlaceholderCloudName = "YOUR_CLOUD_NAME";
        public const string PlaceholderApiKey = "YOUR_API_KEY";
        public const string PlaceholderApiSecret = "YOUR_API_SECRET";

        public string CloudName { get; set; } = PlaceholderCloudName;
        public string ApiKey { get; set; } = PlaceholderApiKey;
        public string ApiSecret { get; set; } = PlaceholderApiSecret;
        public string Folder { get; set; } = "plaza-inn";

        /// <summary>
        /// Real credentials are required. Placeholder values keep uploads on local wwwroot.
        /// </summary>
        public bool IsConfigured =>
            IsReal(CloudName, PlaceholderCloudName) &&
            IsReal(ApiKey, PlaceholderApiKey) &&
            IsReal(ApiSecret, PlaceholderApiSecret);

        private static bool IsReal(string? value, string placeholder) =>
            !string.IsNullOrWhiteSpace(value) &&
            !string.Equals(value.Trim(), placeholder, StringComparison.OrdinalIgnoreCase);
    }
}
