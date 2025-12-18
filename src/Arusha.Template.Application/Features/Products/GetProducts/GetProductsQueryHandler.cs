namespace Arusha.Template.Application.Features.Products.GetProducts;

internal sealed class GetProductsQueryHandler(
    IProductRepository productRepository)
    : IQueryHandler<GetProductsQuery, PagedResponse<ProductListItem>>
{
    public async Task<Result<PagedResponse<ProductListItem>>> Handle(
        GetProductsQuery request,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<Product> products;

        if (!string.IsNullOrEmpty(request.Category))
        {
            products = await productRepository.GetByCategoryAsync(request.Category, cancellationToken);
        }
        else if (request.OnlyActive)
        {
            products = await productRepository.GetAllActiveAsync(cancellationToken);
        }
        else
        {
            // For simplicity, returning active only when no filter
            products = await productRepository.GetAllActiveAsync(cancellationToken);
        }

        // In-memory pagination (in production, use specification pattern with DB pagination)
        var totalCount = products.Count;
        var pagedItems = products
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(p => new ProductListItem(
                p.Id.Value,
                p.Name,
                p.Price,
                p.Currency,
                p.StockQuantity,
                p.IsActive,
                p.Category))
            .ToList();

        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

        return new PagedResponse<ProductListItem>(
            pagedItems,
            request.Page,
            request.PageSize,
            totalCount,
            totalPages);
    }
}
