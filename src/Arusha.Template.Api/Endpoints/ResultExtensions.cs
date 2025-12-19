namespace Arusha.Template.Api.Endpoints;

/// <summary>
/// Extension methods for converting Result to API responses.
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Converts a Result to an ApiResponse with metadata.
    /// </summary>
    public static IResult ToApiResponse<T>(this Result<T> result, HttpContext httpContext)
    {
        var correlationId = GetCorrelationId(httpContext);

        return result.Match(
            data => Results.Ok(new ApiResponse<T>
            {
                CorrelationId = correlationId,
                Data = data,
                Meta = new ApiResponseMeta
                {
                    ServerTime = DateTime.UtcNow.ToString("O"),
                    TraceId = correlationId
                }
            }),
            error => CreateProblemResult(error, httpContext, correlationId));
    }

    /// <summary>
    /// Converts a void Result to an ApiResponse with a success message.
    /// </summary>
    public static IResult ToApiResponse(this Result result, HttpContext httpContext, string successMessage = "Operation completed successfully")
    {
        var correlationId = GetCorrelationId(httpContext);

        return result.Match(
            () => Results.Ok(new ApiResponse<string>
            {
                CorrelationId = correlationId,
                Data = successMessage,
                Meta = new ApiResponseMeta
                {
                    ServerTime = DateTime.UtcNow.ToString("O"),
                    TraceId = correlationId
                }
            }),
            error => CreateProblemResult(error, httpContext, correlationId));
    }

    /// <summary>
    /// Converts a Result to a Created response with ApiResponse envelope.
    /// </summary>
    public static IResult ToCreatedApiResponse<T>(
        this Result<T> result,
        HttpContext httpContext,
        string locationUri)
    {
        var correlationId = GetCorrelationId(httpContext);

        return result.Match(
            data => Results.Created(locationUri, new ApiResponse<T>
            {
                CorrelationId = correlationId,
                Data = data,
                Meta = new ApiResponseMeta
                {
                    ServerTime = DateTime.UtcNow.ToString("O"),
                    TraceId = correlationId
                }
            }),
            error => CreateProblemResult(error, httpContext, correlationId));
    }

    /// <summary>
    /// Converts a Result of PagedResponse to a PagedApiResponse.
    /// </summary>
    public static IResult ToPagedApiResponse<T>(
        this Result<PagedResponse<T>> result,
        HttpContext httpContext)
    {
        var correlationId = GetCorrelationId(httpContext);

        return result.Match(
            pagedData => Results.Ok(new PagedApiResponse<T>
            {
                CorrelationId = correlationId,
                Data = pagedData.Items,
                Meta = new PagedMeta(pagedData.Page, pagedData.PageSize, pagedData.TotalCount)
                {
                    ServerTime = DateTime.UtcNow.ToString("O"),
                    TraceId = correlationId
                }
            }),
            error => CreateProblemResult(error, httpContext, correlationId));
    }

    /// <summary>
    /// Gets the correlation ID from HttpContext.
    /// </summary>
    private static string GetCorrelationId(HttpContext httpContext)
    {
        return httpContext.Items.TryGetValue("CorrelationId", out var value) && value is string correlationId
            ? correlationId
            : httpContext.TraceIdentifier;
    }

    /// <summary>
    /// Creates a ProblemDetails result from an Error.
    /// </summary>
    private static IResult CreateProblemResult(Error error, HttpContext httpContext, string correlationId)
    {
        var statusCode = GetStatusCode(error);

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = error.Code,
            Detail = error.Message,
            Instance = httpContext.Request.Path,
            Type = GetProblemType(statusCode)
        };

        problemDetails.Extensions["traceId"] = correlationId;

        return Results.Problem(problemDetails);
    }

    /// <summary>
    /// Maps Error codes to HTTP status codes.
    /// </summary>
    private static int GetStatusCode(Error error)
    {
        return error.Code switch
        {
            var code when code.Contains("NotFound") => StatusCodes.Status404NotFound,
            var code when code.Contains("Validation") => StatusCodes.Status400BadRequest,
            var code when code.Contains("Conflict") => StatusCodes.Status409Conflict,
            var code when code.Contains("Unauthorized") => StatusCodes.Status401Unauthorized,
            var code when code.Contains("Forbidden") => StatusCodes.Status403Forbidden,
            _ => StatusCodes.Status500InternalServerError
        };
    }

    /// <summary>
    /// Gets the RFC 7807 problem type URI for a status code.
    /// </summary>
    private static string GetProblemType(int statusCode)
    {
        return statusCode switch
        {
            400 => "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            401 => "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            403 => "https://tools.ietf.org/html/rfc7231#section-6.5.3",
            404 => "https://tools.ietf.org/html/rfc7231#section-6.5.4",
            409 => "https://tools.ietf.org/html/rfc7231#section-6.5.8",
            _ => "https://tools.ietf.org/html/rfc7231#section-6.6.1"
        };
    }
}
