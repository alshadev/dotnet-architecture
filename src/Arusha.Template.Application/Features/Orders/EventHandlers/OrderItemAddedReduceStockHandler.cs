using Arusha.Template.Domain.Orders.Events;

namespace Arusha.Template.Application.Features.Orders.EventHandlers;

/// <summary>
/// Reduces product stock when an item is added to an order.
/// This handler runs synchronously within the same transaction as the command handler,
/// ensuring atomicity between order creation and stock reduction.
/// </summary>
internal sealed class OrderItemAddedReduceStockHandler(
    IProductRepository productRepository,
    ILogger<OrderItemAddedReduceStockHandler> logger)
    : INotificationHandler<OrderItemAddedDomainEvent>
{
    public async Task Handle(OrderItemAddedDomainEvent notification, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(
            ProductId.From(notification.ProductId), cancellationToken);

        if (product is null)
        {
            logger.LogWarning("Product {ProductId} not found, skipping stock reduction for Order {OrderId}",
                notification.ProductId, notification.OrderId);
            return;
        }

        product.RemoveStock(notification.Quantity);

        logger.LogInformation(
            "Reduced stock by {Quantity} for Product {ProductId} (Order {OrderId}). Remaining: {Stock}",
            notification.Quantity, notification.ProductId, notification.OrderId, product.StockQuantity);
    }
}
