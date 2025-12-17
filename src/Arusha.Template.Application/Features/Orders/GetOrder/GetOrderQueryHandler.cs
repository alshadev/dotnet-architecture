namespace Arusha.Template.Application.Features.Orders.GetOrder;

/// <summary>
/// Handler for GetOrderQuery.
/// Demonstrates query handler pattern.
/// </summary>
internal sealed class GetOrderQueryHandler(
    IOrderRepository orderRepository)
    : IQueryHandler<GetOrderQuery, OrderResponse>
{
    public async Task<Result<OrderResponse>> Handle(
        GetOrderQuery request,
        CancellationToken cancellationToken)
    {
        var orderId = OrderId.From(request.OrderId);
        var order = await orderRepository.GetByIdWithItemsAsync(orderId, cancellationToken);

        if (order is null)
        {
            return Error.NotFound(
                "Order.NotFound",
                $"Order with ID {request.OrderId} was not found.");
        }

        var response = MapToResponse(order);
        return response;
    }

    private static OrderResponse MapToResponse(Order order)
    {
        return new OrderResponse(
            order.Id.Value,
            order.CustomerId,
            order.Status.Name,
            order.TotalPrice.Amount,
            order.TotalPrice.Currency,
            new AddressResponse(
                order.ShippingAddress.Street,
                order.ShippingAddress.City,
                order.ShippingAddress.State,
                order.ShippingAddress.PostalCode,
                order.ShippingAddress.Country),
            order.Items.Select(item => new OrderItemResponse(
                item.Id.Value,
                item.ProductId,
                item.ProductName,
                item.Quantity,
                item.UnitPrice.Amount,
                item.TotalPrice.Amount,
                item.UnitPrice.Currency)).ToList(),
            order.CreatedOnUtc,
            order.ConfirmedAt,
            order.ShippedAt,
            order.DeliveredAt);
    }
}
