using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Cliniq.API.Exceptions;

namespace Cliniq.API.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
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
        }
        catch (Exception exception)
        {
            if (context.Response.HasStarted)
            {
                _logger.LogError(exception, "An exception occurred after the response started.");
                throw;
            }

            var (statusCode, errorCode, title, detail) = MapException(exception);
            if (statusCode >= 500)
            {
                _logger.LogError(exception, "Unhandled API error for {Path}.", context.Request.Path);
                detail = "An unexpected error occurred.";
            }
            else
            {
                _logger.LogInformation(exception, "API request failed with {StatusCode}.", statusCode);
            }

            context.Response.Clear();
            context.Response.StatusCode = statusCode;
            await context.Response.WriteAsJsonAsync(new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = detail,
                Type = $"https://httpstatuses.com/{statusCode}",
                Extensions = { ["code"] = errorCode }
            });
        }
    }

    private static (int StatusCode, string ErrorCode, string Title, string Detail) MapException(
        Exception exception)
    {
        if (exception is BaseWebException webException)
        {
            return (webException.StatusCode, webException.ErrorCode, "Request failed", webException.Message);
        }

        return exception switch
        {
            ArgumentException => (400, "invalid_request", "Invalid request", exception.Message),
            KeyNotFoundException => (404, "not_found", "Resource not found", exception.Message),
            NotSupportedException => (501, "not_supported", "Operation not supported", exception.Message),
            DbUpdateConcurrencyException => (409, "concurrency_conflict", "Concurrency conflict",
                "The resource was changed by another request."),
            DbUpdateException => (409, "data_conflict", "Data conflict",
                "The request conflicts with existing data."),
            InvalidOperationException => (409, "invalid_operation", "Invalid operation", exception.Message),
            _ => (500, "internal_error", "Internal server error", "An unexpected error occurred.")
        };
    }
}
