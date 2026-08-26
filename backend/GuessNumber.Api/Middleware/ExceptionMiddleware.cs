using System.Text.Json;
using GuessNumber.Application.Exceptions;

namespace GuessNumber.Api.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
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
        catch (AppException ex) // expected business errors -> 400
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await WriteAsync(context, ex.Message);
        }
        catch (Exception ex) // unexpected -> 500
        {
            _logger.LogError(ex, "Unhandled exception");
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await WriteAsync(context, "An unexpected error occurred.");
        }
    }

    private static Task WriteAsync(HttpContext context, string message)
    {
        context.Response.ContentType = "application/json";
        return context.Response.WriteAsync(JsonSerializer.Serialize(new { error = message }));
    }
}