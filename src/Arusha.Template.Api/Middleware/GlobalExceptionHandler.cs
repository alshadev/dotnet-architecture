namespace Arusha.Template.Api.Middleware;

/// <summary>
/// Global exception handling middleware.
/// Converts exceptions to RFC 7807 ProblemDetails responses.
/// </summary>
public sealed class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        logger.LogError(
            exception,
            "Unhandled exception occurred: {Message}",
            exception.Message);

        httpContext.Response.ContentType = "application/problem+json";

        switch (exception)
        {
            case ValidationException validationException:
                await HandleValidationExceptionAsync(httpContext, validationException, cancellationToken);
                break;
            default:
                var problemDetails = CreateProblemDetails(exception, httpContext);
                httpContext.Response.StatusCode = problemDetails.Status ?? (int)HttpStatusCode.InternalServerError;
                await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
                break;
        }

        return true;
    }

    private static async Task HandleValidationExceptionAsync(
        HttpContext httpContext,
        ValidationException validationException,
        CancellationToken cancellationToken)
    {
        var validationProblemDetails = new ValidationProblemDetails(validationException.Errors)
        {
            Status = (int)HttpStatusCode.BadRequest,
            Title = "Validation Error",
            Detail = "One or more validation errors occurred.",
            Instance = httpContext.Request.Path,
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
        };

        httpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
        await httpContext.Response.WriteAsJsonAsync(validationProblemDetails, cancellationToken);
    }

    private static ProblemDetails CreateProblemDetails(Exception exception, HttpContext context)
    {
        return exception switch
        {
            ArgumentException argumentException => new ProblemDetails
            {
                Status = (int)HttpStatusCode.BadRequest,
                Title = "Bad Request",
                Detail = argumentException.Message,
                Instance = context.Request.Path,
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
            },
            InvalidOperationException invalidOpException => new ProblemDetails
            {
                Status = (int)HttpStatusCode.Conflict,
                Title = "Conflict",
                Detail = invalidOpException.Message,
                Instance = context.Request.Path,
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4"
            },
            _ => new ProblemDetails
            {
                Status = (int)HttpStatusCode.InternalServerError,
                Title = "Internal Server Error",
                Detail = "An unexpected error occurred.",
                Instance = context.Request.Path,
                Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1"
            }
        };
    }
}
