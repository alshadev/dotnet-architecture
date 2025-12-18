namespace Arusha.Template.Domain.Orders;

/// <summary>
/// Order aggregate root demonstrating Rich Domain Model.
/// 
/// Key DDD concepts demonstrated:
/// - Aggregate root with child entities (OrderItems)
/// - Value objects (Money, Address)
/// - Smart enumeration (OrderStatus)
/// - Domain events (OrderCreatedDomainEvent, etc.)
/// - Invariant enforcement (business rules)
/// - Factory methods (Create)
/// </summary>
public sealed class Order : AggregateRoot<OrderId>, IAuditableEntity, ISoftDeletable
{
    private readonly List<OrderItem> _items = [];

    public string CustomerId { get; private set; } = default!;
    public Address ShippingAddress { get; private set; } = default!;
    public OrderStatus Status { get; private set; } = default!;
    public DateTime? ConfirmedAt { get; private set; }
    public DateTime? ShippedAt { get; private set; }
    public DateTime? DeliveredAt { get; private set; }
    public DateTime? CancelledAt { get; private set; }
    public string CancellationReason { get; private set; }

    /// <summary>
    /// Auditable properties.
    /// </summary>
    public DateTime CreatedOnUtc { get; set; }
    public string CreatedBy { get; set; }
    public DateTime? ModifiedOnUtc { get; set; }
    public string ModifiedBy { get; set; }

    /// <summary>
    /// Soft-delete properties.
    /// </summary>
    public bool IsDeleted { get; set; }
    public DateTime? DeletedOnUtc { get; set; }
    public string DeletedBy { get; set; }

    /// <summary>
    /// Read-only collection of order items.
    /// Items are managed through aggregate methods.
    /// </summary>
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    /// <summary>
    /// Calculated total price from all items.
    /// </summary>
    public Money TotalPrice
    {
        get
        {
            if (_items.Count == 0)
                return Money.Zero();

            var currency = _items[0].UnitPrice.Currency;
            return _items.Aggregate(Money.Zero(currency), (total, item) => total.Add(item.TotalPrice));
        }
    }

    private Order(
        OrderId id,
        string customerId,
        Address shippingAddress)
        : base(id)
    {
        CustomerId = customerId;
        ShippingAddress = shippingAddress;
        Status = OrderStatus.Pending;
    }

    /// <summary>
    /// Private constructor for EF Core.
    /// </summary>
    private Order() : base() { }

    /// <summary>
    /// Factory method to create a new order.
    /// </summary>
    public static Order Create(string customerId, Address shippingAddress)
    {
        if (string.IsNullOrWhiteSpace(customerId))
            throw new ArgumentException("Customer ID is required.", nameof(customerId));

        ArgumentNullException.ThrowIfNull(shippingAddress);

        var order = new Order(OrderId.New(), customerId, shippingAddress);

        // Raise immediate domain event (processed in same transaction)
        order.RaiseDomainEvent(new Events.OrderCreatedDomainEvent(order.Id));

        return order;
    }

    /// <summary>
    /// Adds an item to the order.
    /// Demonstrates invariant enforcement at aggregate level.
    /// </summary>
    public void AddItem(Guid productId, string productName, int quantity, Money unitPrice)
    {
        EnsureOrderIsModifiable();

        // Check if item already exists
        var existingItem = _items.FirstOrDefault(i => i.ProductId == productId);
        if (existingItem is not null)
        {
            existingItem.UpdateQuantity(existingItem.Quantity + quantity);
        }
        else
        {
            var item = OrderItem.Create(productId, productName, quantity, unitPrice);
            _items.Add(item);
        }

        RaiseDomainEvent(new Events.OrderItemAddedDomainEvent(Id, productId, quantity));
    }

    /// <summary>
    /// Removes an item from the order.
    /// </summary>
    public void RemoveItem(OrderItemId itemId)
    {
        EnsureOrderIsModifiable();

        var item = _items.FirstOrDefault(i => i.Id == itemId)
            ?? throw new InvalidOperationException($"Order item {itemId} not found.");

        _items.Remove(item);
    }

    /// <summary>
    /// Confirms the order.
    /// </summary>
    public void Confirm()
    {
        if (!Status.CanTransitionTo(OrderStatus.Confirmed))
            throw new InvalidOperationException($"Cannot confirm order in {Status} status.");

        if (_items.Count == 0)
            throw new InvalidOperationException("Cannot confirm an order without items.");

        Status = OrderStatus.Confirmed;
        ConfirmedAt = DateTime.UtcNow;

        // Raise integration event (will be processed via outbox)
        RaiseDomainEvent(new Events.OrderConfirmedIntegrationEvent(Id, CustomerId, TotalPrice.Amount));
    }

    /// <summary>
    /// Marks the order as shipped.
    /// </summary>
    public void Ship()
    {
        if (!Status.CanTransitionTo(OrderStatus.Shipped))
            throw new InvalidOperationException($"Cannot ship order in {Status} status.");

        Status = OrderStatus.Shipped;
        ShippedAt = DateTime.UtcNow;

        RaiseDomainEvent(new Events.OrderShippedDomainEvent(Id));
    }

    /// <summary>
    /// Marks the order as delivered.
    /// </summary>
    public void Deliver()
    {
        if (!Status.CanTransitionTo(OrderStatus.Delivered))
            throw new InvalidOperationException($"Cannot deliver order in {Status} status.");

        Status = OrderStatus.Delivered;
        DeliveredAt = DateTime.UtcNow;

        RaiseDomainEvent(new Events.OrderDeliveredIntegrationEvent(Id, CustomerId));
    }

    /// <summary>
    /// Cancels the order.
    /// </summary>
    public void Cancel(string reason)
    {
        if (!Status.CanTransitionTo(OrderStatus.Cancelled))
            throw new InvalidOperationException($"Cannot cancel order in {Status} status.");

        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Cancellation reason is required.", nameof(reason));

        Status = OrderStatus.Cancelled;
        CancelledAt = DateTime.UtcNow;
        CancellationReason = reason;

        RaiseDomainEvent(new Events.OrderCancelledIntegrationEvent(Id, CustomerId, reason));
    }

    /// <summary>
    /// Updates the shipping address.
    /// </summary>
    public void UpdateShippingAddress(Address newAddress)
    {
        EnsureOrderIsModifiable();
        ArgumentNullException.ThrowIfNull(newAddress);

        ShippingAddress = newAddress;
    }

    private void EnsureOrderIsModifiable()
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException($"Cannot modify order in {Status} status.");
    }
}
