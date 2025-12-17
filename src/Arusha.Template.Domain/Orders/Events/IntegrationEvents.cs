namespace Arusha.Template.Domain.Orders.Events;

/// <summary>
/// Integration event raised when an order is confirmed.
/// This is an integration event (processed via outbox, published to event bus).
/// 
/// Integration events are used to notify other bounded contexts/services.
/// They contain all necessary data for external consumers.
/// </summary>
public sealed record OrderConfirmedIntegrationEvent(
    OrderId OrderId,
    string CustomerId,
    decimal TotalAmount) : DomainEvent, IIntegrationEvent;

/// <summary>
/// Integration event raised when an order is delivered.
/// This is an integration event (processed via outbox, published to event bus).
/// </summary>
public sealed record OrderDeliveredIntegrationEvent(
    OrderId OrderId,
    string CustomerId) : DomainEvent, IIntegrationEvent;

/// <summary>
/// Integration event raised when an order is cancelled.
/// This is an integration event (processed via outbox, published to event bus).
/// </summary>
public sealed record OrderCancelledIntegrationEvent(
    OrderId OrderId,
    string CustomerId,
    string Reason) : DomainEvent, IIntegrationEvent;
