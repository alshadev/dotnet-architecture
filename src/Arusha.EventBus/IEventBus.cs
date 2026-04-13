namespace Arusha.EventBus;

public interface IEventBus
{
    Task PublishAsync<T>(T @event) where T : IntegrationEvent;

    Task SubscribeAsync<T, TH>(string serviceName)
        where T : IntegrationEvent
        where TH : IIntegrationEventHandler<T>;

    Task RepublishAsync(IntegrationEventLog eventLog);

    Task ReprocessSubscriptionAsync(IntegrationEventLog eventLog);
}
