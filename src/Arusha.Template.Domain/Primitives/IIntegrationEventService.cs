namespace Arusha.Template.Domain.Primitives;

public interface IIntegrationEventService
{
    void AddIntegrationEvent<T>(T @event) where T : IntegrationEvent;
    Task PublishIntegrationEventAsync();
}
