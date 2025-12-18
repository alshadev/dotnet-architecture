namespace Arusha.Template.Application.Features.Orders.ConfirmOrder;

/// <summary>
/// Handler for ConfirmOrderCommand.
/// </summary>
internal sealed class ConfirmOrderCommandHandler(
    IOrderRepository orderRepository,
    ILogger<ConfirmOrderCommandHandler> logger)
    : ICommandHandler<ConfirmOrderCommand>
{
    public async Task<Result> Handle(
        ConfirmOrderCommand request,
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

        try
        {
            order.Confirm();
            orderRepository.Update(order);

            logger.LogInformation(
                "Order {OrderId} confirmed for customer {CustomerId}",
                order.Id,
                order.CustomerId);

            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(
                ex,
                "Failed to confirm order {OrderId}: {Message}",
                request.OrderId,
                ex.Message);

            return Error.Validation("Order.InvalidStatus", ex.Message);
        }
    }
}
