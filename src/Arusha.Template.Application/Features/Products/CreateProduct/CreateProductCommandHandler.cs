namespace Arusha.Template.Application.Features.Products.CreateProduct;

internal sealed class CreateProductCommandHandler(
    IProductRepository productRepository,
    ILogger<CreateProductCommandHandler> logger)
    : ICommandHandler<CreateProductCommand, Guid>
{
    public async Task<Result<Guid>> Handle(
        CreateProductCommand request,
        CancellationToken cancellationToken)
    {
        // Check for duplicate SKU if provided
        if (!string.IsNullOrEmpty(request.Sku))
        {
            var existingProduct = await productRepository.ExistsBySkuAsync(request.Sku, cancellationToken);
            if (existingProduct)
            {
                return Error.Conflict(
                    "Product.DuplicateSku",
                    $"A product with SKU '{request.Sku}' already exists.");
            }
        }

        var product = Product.Create(
            request.Name,
            request.Description,
            request.Price,
            request.Currency,
            request.StockQuantity,
            request.Sku,
            request.Category);

        productRepository.Add(product);

        logger.LogInformation(
            "Created product {ProductId} with name '{ProductName}'",
            product.Id,
            product.Name);

        return product.Id.Value;
    }
}
