namespace Arusha.Template.Domain.Primitives;

public interface IIntegrationEventHandler<T> where T : IntegrationEvent
{
    Task Handle(T @event);
}
