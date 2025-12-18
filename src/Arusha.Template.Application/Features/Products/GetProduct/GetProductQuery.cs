namespace Arusha.Template.Application.Features.Products.GetProduct;

public sealed record GetProductQuery(Guid ProductId) : IQuery<ProductResponse>;

public sealed record ProductResponse(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    string Currency,
    int StockQuantity,
    bool IsActive,
    string Sku,
    string Category,
    DateTime CreatedAt,
    DateTime? ModifiedAt);
