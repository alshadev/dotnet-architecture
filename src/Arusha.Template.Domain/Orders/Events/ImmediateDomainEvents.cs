namespace Arusha.Template.Domain.Orders.Events;

/// <summary>
/// Domain event raised when a new order is created.
/// Dispatched synchronously before SaveChanges (same transaction).
/// </summary>
public sealed record OrderCreatedDomainEvent(OrderId OrderId) : DomainEvent;

/// <summary>
/// Domain event raised when an item is added to an order.
/// Dispatched synchronously before SaveChanges (same transaction).
/// </summary>
public sealed record OrderItemAddedDomainEvent(
    OrderId OrderId,
    Guid ProductId,
    int Quantity) : DomainEvent;

/// <summary>
/// Domain event raised when an order is shipped.
/// Dispatched synchronously before SaveChanges (same transaction).
/// </summary>
public sealed record OrderShippedDomainEvent(OrderId OrderId) : DomainEvent;

/// <summary>
/// Domain event raised when an order is confirmed.
/// </summary>
public sealed record OrderConfirmedDomainEvent(
    OrderId OrderId,
    string CustomerId,
    decimal TotalAmount) : DomainEvent;

/// <summary>
/// Domain event raised when an order is delivered.
/// </summary>
public sealed record OrderDeliveredDomainEvent(
    OrderId OrderId,
    string CustomerId) : DomainEvent;

/// <summary>
/// Domain event raised when an order is cancelled.
/// </summary>
public sealed record OrderCancelledDomainEvent(
    OrderId OrderId,
    string CustomerId,
    string Reason) : DomainEvent;
