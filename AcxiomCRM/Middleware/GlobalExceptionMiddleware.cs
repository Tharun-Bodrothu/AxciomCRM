#nullable enable
using System.Net;
using System.Text.Json;

namespace AcxiomCRM.Middleware;

/// <summary>
/// Global exception handling middleware for production environments.
/// Catches unhandled exceptions, logs them, and returns user-friendly error responses.
/// </summary>
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred while processing {Method} {Path}",
                context.Request.Method, context.Request.Path);

            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, message) = exception switch
        {
            UnauthorizedAccessException => (HttpStatusCode.Forbidden, "You do not have permission to perform this action."),
            KeyNotFoundException => (HttpStatusCode.NotFound, "The requested resource was not found."),
            ArgumentException => (HttpStatusCode.BadRequest, "Invalid request parameters."),
            InvalidOperationException => (HttpStatusCode.BadRequest, "The requested operation is not valid."),
            _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred. Please try again later.")
        };

        context.Response.StatusCode = (int)statusCode;

        // For API requests, return JSON
        if (context.Request.Path.StartsWithSegments("/api"))
        {
            var result = JsonSerializer.Serialize(new
            {
                success = false,
                message,
                statusCode = (int)statusCode
            });
            await context.Response.WriteAsync(result);
        }
        else
        {
            // For MVC requests, redirect to error page
            context.Response.Redirect("/Home/Error");
        }
    }
}
