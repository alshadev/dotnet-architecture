namespace Arusha.Template.Domain.Abstractions;

/// <summary>
/// Marker interface for integration events.
/// These events are persisted to the Outbox table in the same transaction,
/// then published asynchronously to external systems via IEventBus.
/// Use for: cross-service communication, external system notifications.
/// </summary>
public interface IIntegrationEvent : IDomainEvent
{
}
