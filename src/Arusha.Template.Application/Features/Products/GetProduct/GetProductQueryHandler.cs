namespace Arusha.Template.Application.Features.Products.GetProduct;

internal sealed class GetProductQueryHandler(
    IProductRepository productRepository)
    : IQueryHandler<GetProductQuery, ProductResponse>
{
    public async Task<Result<ProductResponse>> Handle(
        GetProductQuery request,
        CancellationToken cancellationToken)
    {
        var productId = ProductId.From(request.ProductId);
        var product = await productRepository.GetByIdAsync(productId, cancellationToken);

        if (product is null)
        {
            return Error.NotFound(
                "Product.NotFound",
                $"Product with ID {request.ProductId} was not found.");
        }

        return new ProductResponse(
            product.Id.Value,
            product.Name,
            product.Description,
            product.Price,
            product.Currency,
            product.StockQuantity,
            product.IsActive,
            product.Sku,
            product.Category,
            product.CreatedOnUtc,
            product.ModifiedOnUtc);
    }
}
