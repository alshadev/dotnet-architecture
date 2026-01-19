namespace Arusha.Template.Application.Results;

/// <summary>
/// Paged response wrapper for paginated queries.
/// </summary>
public sealed record PagedResponse<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages);