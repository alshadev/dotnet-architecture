namespace Arusha.Template.Application.Features.Products.CreateProduct;

/// <summary>
/// Command to create a new product.
/// Simple CRUD operation for pragmatic DDD.
/// </summary>
public sealed record CreateProductCommand(
    string Name,
    string Description,
    decimal Price,
    string Currency,
    int StockQuantity,
    string Sku,
    string Category) : ICommand<Guid>;
