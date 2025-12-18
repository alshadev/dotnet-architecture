namespace Arusha.Template.Application.Features.Products.GetProducts;

public sealed record GetProductsQuery(
    string Category = null,
    bool OnlyActive = true,
    int Page = 1,
    int PageSize = 10) : IQuery<PagedResponse<ProductListItem>>;

public sealed record ProductListItem(
    Guid Id,
    string Name,
    decimal Price,
    string Currency,
    int StockQuantity,
    bool IsActive,
    string Category);

public sealed record PagedResponse<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages);
