using Arusha.Template.Domain.IntegrationEventLogs;

namespace Arusha.Template.Infrastructure.EventBus;

/// <summary>
/// In-memory event bus implementation.
/// Uses MediatR to dispatch events within the same process.
/// 
/// Suitable for:
/// - Development and testing
/// - Single-instance deployments
/// - When external message broker is not needed
/// 
/// Replace with RabbitMQ/Azure Service Bus for production
/// distributed scenarios.
/// </summary>
public sealed class InMemoryEventBus(
    IMediator mediator,
    ILogger<InMemoryEventBus> logger) : IEventBus
{
    public Task PublishAsync<T>(T @event) where T : IntegrationEvent => throw new NotImplementedException();
    public Task ReprocessSubscriptionAsync<T>(T eventLog) where T : IntegrationEventLog => throw new NotImplementedException();
    public Task RepublishAsync<T>(T eventLog) where T : IntegrationEventLog => throw new NotImplementedException();
    public Task SubscribeAsync<T, TH>(string serviceName)
        where T : IntegrationEvent
        where TH : IIntegrationEventHandler<T> => throw new NotImplementedException();
}
