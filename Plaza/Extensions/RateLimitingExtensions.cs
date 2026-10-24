using System.Diagnostics;
using System.Globalization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Plaza.Extensions
{
    public static class RateLimitingExtensions
    {
        /// <summary>Login, register, forgot/reset password: 10 requests per minute per client IP.</summary>
        public const string AuthPolicy = "auth";

        /// <summary>Booking creation: 5 requests per 10 minutes per user (or client IP when unauthenticated).</summary>
        public const string BookingCreationPolicy = "booking-create";

        public static IServiceCollection AddPlazaRateLimiting(this IServiceCollection services)
        {
            services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
                options.OnRejected = WriteRejectionAsync;

                options.AddPolicy(AuthPolicy, httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        $"ip:{GetClientIp(httpContext)}",
                        _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 10,
                            Window = TimeSpan.FromMinutes(1),
                            QueueLimit = 0
                        }));

                options.AddPolicy(BookingCreationPolicy, httpContext =>
                {
                    var userId = httpContext.User.GetUserId();
                    var partitionKey = string.IsNullOrWhiteSpace(userId) ? $"ip:{GetClientIp(httpContext)}" : $"user:{userId}";

                    return RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey,
                        _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 5,
                            Window = TimeSpan.FromMinutes(10),
                            QueueLimit = 0
                        });
                });
            });

            return services;
        }

        private static string GetClientIp(HttpContext httpContext) =>
            httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        private static async ValueTask WriteRejectionAsync(OnRejectedContext context, CancellationToken cancellationToken)
        {
            var httpContext = context.HttpContext;

            if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                httpContext.Response.Headers.RetryAfter = ((int)Math.Ceiling(retryAfter.TotalSeconds)).ToString(CultureInfo.InvariantCulture);

            const string detail = "Too many requests. Please try again later.";
            var problem = new ProblemDetails
            {
                Status = StatusCodes.Status429TooManyRequests,
                Title = "Too many requests",
                Detail = detail,
                Instance = httpContext.Request.Path
            };
            problem.Extensions["traceId"] = Activity.Current?.Id ?? httpContext.TraceIdentifier;
            problem.Extensions["message"] = detail;

            await httpContext.Response.WriteAsJsonAsync(problem, options: null, contentType: "application/problem+json", cancellationToken);
        }
    }
}
