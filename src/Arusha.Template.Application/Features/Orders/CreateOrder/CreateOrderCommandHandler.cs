namespace Arusha.Template.Application.Features.Orders.CreateOrder;

/// <summary>
/// Handler for CreateOrderCommand.
/// Demonstrates the REPR pattern (Request-Endpoint-Presenter-Response).
/// 
/// Note: Handler does NOT call SaveChanges.
/// TransactionBehavior handles the commit.
/// </summary>
internal sealed class CreateOrderCommandHandler(
    IOrderRepository orderRepository,
    ILogger<CreateOrderCommandHandler> logger)
    : ICommandHandler<CreateOrderCommand, Guid>
{
    public async Task<Result<Guid>> Handle(
        CreateOrderCommand request,
        CancellationToken cancellationToken)
    {
        var address = Address.Create(
            request.ShippingAddress.Street,
            request.ShippingAddress.City,
            request.ShippingAddress.State,
            request.ShippingAddress.PostalCode,
            request.ShippingAddress.Country);

        var order = Order.Create(request.CustomerId, address);

        foreach (var item in request.Items)
        {
            var unitPrice = Money.Create(item.UnitPrice, item.Currency);
            order.AddItem(item.ProductId, item.ProductName, item.Quantity, unitPrice);
        }

        orderRepository.Add(order);

        logger.LogInformation(
            "Created order {OrderId} for customer {CustomerId} with {ItemCount} items",
            order.Id,
            request.CustomerId,
            request.Items.Count);

        // Return success - SaveChanges is called by TransactionBehavior
        return order.Id.Value;
    }
}
