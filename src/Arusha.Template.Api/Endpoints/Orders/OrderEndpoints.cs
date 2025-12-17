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
        var correlationId = httpContext.Items["CorrelationId"] as string ?? httpContext.TraceIdentifier;
        var result = await sender.Send(command, cancellationToken);

        return result.Match(
            orderId => Results.Created($"/api/v1/orders/{orderId}", ApiResponse<Guid>.Ok(orderId, correlationId)),
            error => Results.Json(
                ApiResponse<string>.Fail(correlationId, error.Code, error.Message, GetStatusCode(error)),
                statusCode: GetStatusCode(error)));
    }

    private static async Task<IResult> GetOrder(
        Guid orderId,
        ISender sender,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var correlationId = httpContext.Items["CorrelationId"] as string ?? httpContext.TraceIdentifier;
        var query = new GetOrderQuery(orderId);
        var result = await sender.Send(query, cancellationToken);

        return result.Match(
            order => Results.Ok(ApiResponse<OrderResponse>.Ok(order, correlationId)),
            error => Results.Json(
                ApiResponse<string>.Fail(correlationId, error.Code, error.Message, GetStatusCode(error)),
                statusCode: GetStatusCode(error)));
    }

    private static async Task<IResult> ConfirmOrder(
        Guid orderId,
        ISender sender,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var correlationId = httpContext.Items["CorrelationId"] as string ?? httpContext.TraceIdentifier;
        var command = new ConfirmOrderCommand(orderId);
        var result = await sender.Send(command, cancellationToken);

        return result.Match(
            () => Results.Ok(ApiResponse<string>.Ok("Confirmed", correlationId)),
            error => Results.Json(
                ApiResponse<string>.Fail(correlationId, error.Code, error.Message, GetStatusCode(error)),
                statusCode: GetStatusCode(error)));
    }

    private static int GetStatusCode(Error error)
    {
        return error.Code switch
        {
            var code when code.Contains("NotFound") => StatusCodes.Status404NotFound,
            var code when code.Contains("Validation") => StatusCodes.Status400BadRequest,
            var code when code.Contains("Conflict") => StatusCodes.Status409Conflict,
            var code when code.Contains("Unauthorized") => StatusCodes.Status401Unauthorized,
            var code when code.Contains("Forbidden") => StatusCodes.Status403Forbidden,
            _ => StatusCodes.Status500InternalServerError
        };
    }
}
