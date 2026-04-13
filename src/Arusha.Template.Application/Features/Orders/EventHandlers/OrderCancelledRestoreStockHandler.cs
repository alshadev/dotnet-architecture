using Arusha.Template.Domain.Orders.Events;

namespace Arusha.Template.Application.Features.Orders.EventHandlers;

/// <summary>
/// Restores product stock when an order is cancelled.
/// This handler runs synchronously within the same transaction as the command handler,
/// ensuring atomicity between order cancellation and stock restoration.
/// </summary>
internal sealed class OrderCancelledRestoreStockHandler(
    IOrderRepository orderRepository,
    IProductRepository productRepository,
    ILogger<OrderCancelledRestoreStockHandler> logger)
    : INotificationHandler<OrderCancelledDomainEvent>
{
    public async Task Handle(OrderCancelledDomainEvent notification, CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetByIdWithItemsAsync(notification.OrderId, cancellationToken);

        if (order is null)
        {
            logger.LogWarning("Order {OrderId} not found, skipping stock restoration", notification.OrderId);
            return;
        }

        foreach (var item in order.Items)
        {
            var product = await productRepository.GetByIdAsync(
                ProductId.From(item.ProductId), cancellationToken);

            if (product is null)
            {
                logger.LogWarning("Product {ProductId} not found, skipping stock restoration for this item",
                    item.ProductId);
                continue;
            }

            product.AddStock(item.Quantity);

            logger.LogInformation(
                "Restored stock by {Quantity} for Product {ProductId} (Order {OrderId} cancelled). Current: {Stock}",
                item.Quantity, item.ProductId, notification.OrderId, product.StockQuantity);
        }
    }
}
