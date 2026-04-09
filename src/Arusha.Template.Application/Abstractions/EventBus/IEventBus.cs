using Arusha.Template.Domain.IntegrationEventLogs;

namespace Arusha.Template.Application.Abstractions.EventBus;

/// <summary>
/// Abstraction for publishing integration events.
/// Implementations can use different message brokers:
/// - InMemoryEventBus (default, for development)
/// - RabbitMqEventBus
/// - AzureServiceBusEventBus
/// </summary>
//public interface IEventBus
//{
//    /// <summary>
//    /// Publishes an integration event to subscribers.
//    /// </summary>
//    Task PublishAsync<T>(T integrationEvent, CancellationToken cancellationToken = default)
//        where T : IDomainEvent;
//}

public interface IEventBus
{
    Task PublishAsync<T>(T @event)
        where T : IntegrationEvent;

    Task SubscribeAsync<T, TH>(string serviceName)
        where T : IntegrationEvent
        where TH : IIntegrationEventHandler<T>;

    Task RepublishAsync<T>(T eventLog)
        where T : IntegrationEventLog;

    Task ReprocessSubscriptionAsync<T>(T eventLog)
        where T : IntegrationEventLog;
}
