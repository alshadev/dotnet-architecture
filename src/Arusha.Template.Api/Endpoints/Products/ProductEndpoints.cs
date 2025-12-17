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
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);

        return result.Match(
            productId => Results.Created($"/api/v1/products/{productId}", productId),
            error => Results.Problem(
                detail: error.Message,
                statusCode: GetStatusCode(error),
                title: error.Code));
    }

    private static async Task<IResult> GetProduct(
        Guid productId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var query = new GetProductQuery(productId);
        var result = await sender.Send(query, cancellationToken);

        return result.Match(
            product => Results.Ok(product),
            error => Results.Problem(
                detail: error.Message,
                statusCode: GetStatusCode(error),
                title: error.Code));
    }

    private static async Task<IResult> GetProducts(
        [AsParameters] GetProductsQuery query,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(query, cancellationToken);

        return result.Match(
            products => Results.Ok(products),
            error => Results.Problem(
                detail: error.Message,
                statusCode: GetStatusCode(error),
                title: error.Code));
    }

    private static int GetStatusCode(Error error)
    {
        return error.Code switch
        {
            var code when code.Contains("NotFound") => StatusCodes.Status404NotFound,
            var code when code.Contains("Validation") => StatusCodes.Status400BadRequest,
            var code when code.Contains("Conflict") => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError
        };
    }
}
