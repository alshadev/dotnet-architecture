namespace Arusha.Template.Domain.Orders.Events;

/// <summary>
/// Domain event raised when a new order is created.
/// This is an immediate event (processed in same transaction).
/// </summary>
public sealed record OrderCreatedDomainEvent(OrderId OrderId) : DomainEvent, IImmediateDomainEvent;

/// <summary>
/// Domain event raised when an item is added to an order.
/// This is an immediate event (processed in same transaction).
/// </summary>
public sealed record OrderItemAddedDomainEvent(
    OrderId OrderId,
    Guid ProductId,
    int Quantity) : DomainEvent, IImmediateDomainEvent;

/// <summary>
/// Domain event raised when an order is shipped.
/// This is an immediate event (processed in same transaction).
/// </summary>
public sealed record OrderShippedDomainEvent(OrderId OrderId) : DomainEvent, IImmediateDomainEvent;
