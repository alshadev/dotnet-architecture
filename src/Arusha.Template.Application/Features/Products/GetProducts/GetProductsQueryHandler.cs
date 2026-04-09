namespace Arusha.Template.Application.Features.Products.GetProducts;

internal sealed class GetProductsQueryHandler(
    IProductRepository productRepository)
    : IQueryHandler<GetProductsQuery, PagedResponse<ProductListItem>>
{
    public async Task<Result<PagedResponse<ProductListItem>>> Handle(
        GetProductsQuery request,
        CancellationToken cancellationToken)
    {
        var (products, totalCount) = await productRepository.GetPagedAsync(
            request.Category,
            request.OnlyActive,
            request.Page,
            request.PageSize,
            cancellationToken);

        var items = products
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
            items,
            request.Page,
            request.PageSize,
            totalCount,
            totalPages);
    }
}
