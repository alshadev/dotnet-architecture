namespace Arusha.EventBus;

public interface IIntegrationEventService
{
    void AddIntegrationEvent<T>(T @event) where T : IntegrationEvent;
    Task PublishIntegrationEventAsync();
}
