using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace SpecFirst.Service.Api;

/// <summary>
/// An unhandled exception becomes a 500 problem details response that carries no exception details (TECH-003).
/// The details go to the log.
/// </summary>
internal sealed class GlobalExceptionHandler(IProblemDetailsService problems, ILogger<GlobalExceptionHandler> logger)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken
    )
    {
        var unreadableRequest = exception as BadHttpRequestException;
        if (unreadableRequest is null)
            logger.LogError(
                exception,
                "Unhandled exception on {Method} {Path}",
                httpContext.Request.Method,
                httpContext.Request.Path
            );

        httpContext.Response.StatusCode = unreadableRequest?.StatusCode ?? StatusCodes.Status500InternalServerError;
        return await problems.TryWriteAsync(
            new ProblemDetailsContext
            {
                HttpContext = httpContext,
                ProblemDetails = new ProblemDetails
                {
                    Status = httpContext.Response.StatusCode,
                    Title = unreadableRequest is null
                        ? "An unexpected error occurred."
                        : "The request could not be read.",
                },
            }
        );
    }
}
