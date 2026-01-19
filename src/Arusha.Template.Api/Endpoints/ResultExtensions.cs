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
    /// Converts a Result to a Created response with ApiResponse envelope.
    /// The location URI is built using the result value only on success.
    /// </summary>
    public static IResult ToCreatedApiResponse<T>(
        this Result<T> result,
        HttpContext httpContext,
        Func<T, string> locationUriFactory)
    {
        var correlationId = GetCorrelationId(httpContext);

        return result.Match(
            data => Results.Created(locationUriFactory(data), new ApiResponse<T>
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
        var statusCode = MapErrorTypeToStatusCode(error.Type);

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

    private static int MapErrorTypeToStatusCode(ErrorType errorType) => errorType switch
    {
        // 400 Bad Request
        ErrorType.Validation or
        ErrorType.InvalidInput or
        ErrorType.InvalidOperation or
        ErrorType.BusinessRuleViolation => StatusCodes.Status400BadRequest,

        // 404 Not Found
        ErrorType.NotFound => StatusCodes.Status404NotFound,

        // 409 Conflict
        ErrorType.Conflict or
        ErrorType.AlreadyExists or
        ErrorType.ConcurrencyConflict => StatusCodes.Status409Conflict,

        // 401 Unauthorized
        ErrorType.Unauthorized or
        ErrorType.TokenExpired or
        ErrorType.InvalidCredentials => StatusCodes.Status401Unauthorized,

        // 403 Forbidden
        ErrorType.Forbidden or
        ErrorType.InsufficientPermissions => StatusCodes.Status403Forbidden,

        // 422 Unprocessable Entity
        ErrorType.Unprocessable => StatusCodes.Status422UnprocessableEntity,

        // 503 Service Unavailable
        ErrorType.ServiceUnavailable => StatusCodes.Status503ServiceUnavailable,

        // 500 Internal Server Error (default)
        ErrorType.Failure or _ => StatusCodes.Status500InternalServerError
    };

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
