using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Plaza.Exceptions;

namespace Plaza.MiddelWare
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
            {
                _logger.LogDebug("Request {Method} {Path} was aborted by the client.", context.Request.Method, context.Request.Path);
            }
            catch (Exception ex)
            {
                if (context.Response.HasStarted)
                {
                    _logger.LogError(ex, "Unhandled exception after the response started for {Method} {Path}.",
                        context.Request.Method, context.Request.Path);
                    throw;
                }

                await WriteProblemDetailsAsync(context, ex);
            }
        }

        private async Task WriteProblemDetailsAsync(HttpContext context, Exception ex)
        {
            var (status, title, detail) = ex switch
            {
                NotFoundException => (StatusCodes.Status404NotFound, "Resource not found", ex.Message),
                ForbiddenException => (StatusCodes.Status403Forbidden, "Forbidden", ex.Message),
                ConflictException => (StatusCodes.Status409Conflict, "Conflict", ex.Message),
                ArgumentException => (StatusCodes.Status400BadRequest, "Invalid request", ex.Message),
                BadHttpRequestException badRequest => (badRequest.StatusCode, "Invalid request", ex.Message),
                PaymentGatewayException => (StatusCodes.Status502BadGateway, "Payment gateway error", ex.Message),
                _ => (StatusCodes.Status500InternalServerError, "Internal server error", "An unexpected error occurred. Please try again later.")
            };

            if (status >= StatusCodes.Status500InternalServerError)
                _logger.LogError(ex, "Request {Method} {Path} failed with {StatusCode}.", context.Request.Method, context.Request.Path, status);
            else
                _logger.LogWarning("Request {Method} {Path} failed with {StatusCode}: {Message}", context.Request.Method, context.Request.Path, status, ex.Message);

            var problem = new ProblemDetails
            {
                Status = status,
                Title = title,
                Detail = detail,
                Instance = context.Request.Path
            };
            problem.Extensions["traceId"] = Activity.Current?.Id ?? context.TraceIdentifier;
            // Kept for backward compatibility with clients that read the previous { message } error shape.
            problem.Extensions["message"] = detail;

            context.Response.Clear();
            context.Response.StatusCode = status;
            await context.Response.WriteAsJsonAsync(problem, options: null, contentType: "application/problem+json");
        }
    }

}
