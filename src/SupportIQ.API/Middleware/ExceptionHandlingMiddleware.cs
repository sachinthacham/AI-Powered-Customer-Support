using Microsoft.AspNetCore.Mvc;
using SupportIQ.Application.Common.Exceptions;
using SupportIQ.Domain.Exceptions;
using ValidationException = SupportIQ.Application.Common.Exceptions.ValidationException;

namespace SupportIQ.API.Middleware;

/// <summary>
/// Translates exceptions into RFC 7807 <see cref="ProblemDetails"/> responses so callers get a
/// consistent error shape and the right HTTP status code no matter which layer threw. Registered
/// first in the pipeline so it can catch anything downstream, including AI/RAG failures.
/// </summary>
public class ExceptionHandlingMiddleware : IMiddleware
{
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(ILogger<ExceptionHandlingMiddleware> logger)
    {
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            await HandleAsync(context, exception);
        }
    }

    private async Task HandleAsync(HttpContext context, Exception exception)
    {
        var (statusCode, title) = exception switch
        {
            ValidationException => (StatusCodes.Status422UnprocessableEntity, "Validation failed"),
            NotFoundException => (StatusCodes.Status404NotFound, "Resource not found"),
            UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Unauthorized"),
            InvalidTicketStateException => (StatusCodes.Status409Conflict, "Invalid ticket state"),
            AIServiceException => (StatusCodes.Status502BadGateway, "AI service error"),
            ExternalServiceException => (StatusCodes.Status503ServiceUnavailable, "External service unavailable"),
            ArgumentException => (StatusCodes.Status400BadRequest, "Invalid request"),
            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred")
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
            _logger.LogError(exception, "Unhandled exception processing {Path}", context.Request.Path);
        else
            _logger.LogWarning("{ExceptionType} handled as {StatusCode} for {Path}: {Message}",
                exception.GetType().Name, statusCode, context.Request.Path, exception.Message);

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = statusCode == StatusCodes.Status500InternalServerError
                ? "An unexpected error occurred. Please try again later."
                : exception.Message,
            Instance = context.Request.Path
        };

        if (exception is ValidationException validationException)
            problemDetails.Extensions["errors"] = validationException.Errors;

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsJsonAsync(problemDetails);
    }
}
