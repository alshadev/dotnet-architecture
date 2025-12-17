namespace Arusha.Template.Api.Contracts;

/// <summary>
/// Standard API response envelope.
/// </summary>
public sealed record ApiResponse<T>(bool Success, T Data, ApiError Error, string CorrelationId)
{
    public static ApiResponse<T> Ok(T data, string correlationId) => new(true, data, null, correlationId);
    public static ApiResponse<T> Fail(string correlationId, string code, string message, int statusCode = StatusCodes.Status400BadRequest)
        => new(false, default, new ApiError(code, message, statusCode), correlationId);
}

public sealed record ApiError(string Code, string Message, int StatusCode);
