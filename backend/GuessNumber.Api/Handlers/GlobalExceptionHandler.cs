using GuessNumber.Application.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace GuessNumber.Api.Handlers;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly IProblemDetailsService _problemDetails;
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(IProblemDetailsService problemDetails,
                                  ILogger<GlobalExceptionHandler> logger)
    {
        _problemDetails = problemDetails;
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext ctx, Exception ex, CancellationToken ct)
    {
        // Map known exception types to status codes
        var (status, title) = ex switch
        {
            AppException      => (StatusCodes.Status400BadRequest, ex.Message),
            KeyNotFoundException => (StatusCodes.Status404NotFound, "Resource not found"),
            UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Unauthorized"),
            _                 => (StatusCodes.Status500InternalServerError, "An unexpected error occurred.")
        };

        // Log the FULL exception server-side (never sent to the client)
        if (status == StatusCodes.Status500InternalServerError)
            _logger.LogError(ex, "Unhandled exception. TraceId: {TraceId}", ctx.TraceIdentifier);
        else
            _logger.LogWarning("Handled {Type}: {Message}", ex.GetType().Name, ex.Message);

        ctx.Response.StatusCode = status;

        return await _problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = ctx,
            ProblemDetails = new ProblemDetails
            {
                Status = status,
                Title = title,
                // In prod: never expose ex.ToString() for 500s
                Detail = status == 500 ? null : ex.Message
            }
        });
    }
}