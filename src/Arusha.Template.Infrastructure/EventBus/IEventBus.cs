namespace Arusha.Template.Infrastructure.EventBus;

/// <summary>
/// Abstraction for publishing integration events.
/// Implementations can use different message brokers:
/// - InMemoryEventBus (default, for development)
/// - RabbitMqEventBus
/// - AzureServiceBusEventBus
/// </summary>
public interface IEventBus
{
    /// <summary>
    /// Publishes an integration event to subscribers.
    /// </summary>
    Task PublishAsync<T>(T integrationEvent, CancellationToken cancellationToken = default)
        where T : IDomainEvent;
}
