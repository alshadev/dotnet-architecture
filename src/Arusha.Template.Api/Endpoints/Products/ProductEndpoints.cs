namespace Arusha.Template.Api.Endpoints.Products;

/// <summary>
/// Product endpoints module.
/// Demonstrates simple CRUD endpoints for pragmatic DDD entity.
/// </summary>
public sealed class ProductEndpoints : IEndpointModule
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v{version:apiVersion}/products")
            .WithApiVersionSet(app.NewApiVersionSet().HasApiVersion(new ApiVersion(1, 0)).Build())
            .WithTags("Products");

        group.MapPost("/", CreateProduct)
            .WithName("CreateProduct")
            .WithSummary("Creates a new product")
            .Produces<Guid>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapGet("/{productId:guid}", GetProduct)
            .WithName("GetProduct")
            .WithSummary("Gets a product by ID")
            .Produces<ProductResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapGet("/", GetProducts)
            .WithName("GetProducts")
            .WithSummary("Gets a list of products")
            .Produces<PagedResponse<ProductListItem>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
    }

    private static async Task<IResult> CreateProduct(
        CreateProductCommand command,
        ISender sender,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);

        return result.ToCreatedApiResponse(
            httpContext,
            id => $"/api/v1/products/{id}");
    }

    private static async Task<IResult> GetProduct(
        Guid productId,
        ISender sender,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var query = new GetProductQuery(productId);
        var result = await sender.Send(query, cancellationToken);

        return result.ToApiResponse(httpContext);
    }

    private static async Task<IResult> GetProducts(
        [AsParameters] GetProductsQuery query,
        ISender sender,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(query, cancellationToken);

        return result.ToPagedApiResponse(httpContext);
    }

}
