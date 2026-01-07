namespace Arusha.Template.Api.Endpoints.Orders;

/// <summary>
/// Order endpoints module.
/// Demonstrates manual Minimal API endpoint organization.
/// </summary>
public sealed class OrderEndpoints : IEndpointModule
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v{version:apiVersion}/orders")
            .WithApiVersionSet(app.NewApiVersionSet().HasApiVersion(new ApiVersion(1, 0)).Build())
            .WithTags("Orders")
            .RequireAuthorization();

        group.MapPost("/", CreateOrder)
            .WithName("CreateOrder")
            .WithSummary("Creates a new order")
            .Produces<Guid>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapGet("/{orderId:guid}", GetOrder)
            .WithName("GetOrder")
            .WithSummary("Gets an order by ID")
            .Produces<OrderResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapPost("/{orderId:guid}/confirm", ConfirmOrder)
            .WithName("ConfirmOrder")
            .WithSummary("Confirms an order")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
    }

    private static async Task<IResult> CreateOrder(
        CreateOrderCommand command,
        ISender sender,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);

        return result.ToCreatedApiResponse(
            httpContext,
            $"/api/v1/orders/{result.Value}");
    }

    private static async Task<IResult> GetOrder(
        Guid orderId,
        ISender sender,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var query = new GetOrderQuery(orderId);
        var result = await sender.Send(query, cancellationToken);

        return result.ToApiResponse(httpContext);
    }

    private static async Task<IResult> ConfirmOrder(
        Guid orderId,
        ISender sender,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var command = new ConfirmOrderCommand(orderId);
        var result = await sender.Send(command, cancellationToken);

        return result.ToApiResponse(httpContext, "Order confirmed successfully");
    }

}
