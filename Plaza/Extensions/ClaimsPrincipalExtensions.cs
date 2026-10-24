using System.Security.Claims;

namespace Plaza.Extensions
{
    /// <summary>Reads the custom claim names issued by <c>JwtTokenGenerator</c>.</summary>
    public static class ClaimsPrincipalExtensions
    {
        public static string? GetUserId(this ClaimsPrincipal user) =>
            user.Identity?.IsAuthenticated == true ? user.FindFirst("Id")?.Value : null;

        public static string? GetEmail(this ClaimsPrincipal user) =>
            user.Identity?.IsAuthenticated == true ? user.FindFirst("Email")?.Value : null;

        public static bool IsAdmin(this ClaimsPrincipal user) => user.IsInRole("Admin");
    }
}
