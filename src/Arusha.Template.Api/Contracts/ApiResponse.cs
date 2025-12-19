namespace Arusha.Template.Api.Contracts;

/// <summary>
/// Base metadata for all API responses.
/// </summary>
public record ApiResponseMeta
{
    public string ServerTime { get; init; } = DateTime.UtcNow.ToString("O");
    public string TraceId { get; init; } = string.Empty;
}

/// <summary>
/// Pagination metadata extending base metadata.
/// </summary>
public sealed record PagedMeta : ApiResponseMeta
{
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalRecords { get; init; }
    public int TotalPages { get; init; }

    public PagedMeta(int page, int pageSize, int totalRecords)
    {
        Page = page;
        PageSize = pageSize;
        TotalRecords = totalRecords;
        TotalPages = (int)Math.Ceiling((double)totalRecords / pageSize);
    }
}

/// <summary>
/// Standard API response envelope for success responses.
/// </summary>
public record ApiResponse<T>
{
    public string CorrelationId { get; init; } = string.Empty;
    public T Data { get; init; } = default!;
    public ApiResponseMeta Meta { get; init; } = new();
}

/// <summary>
/// API response envelope for paginated responses.
/// </summary>
public sealed record PagedApiResponse<T> : ApiResponse<IReadOnlyList<T>>
{
    public new PagedMeta Meta { get; init; } = null!;
}
