namespace Arusha.Template.Infrastructure.Integration;

public class IntegrationEventService : IIntegrationEventService
{
    private readonly IEventBus _eventHub;
    private readonly List<(Type Type, IntegrationEvent Event)> _pendingEvents = [];

    public IntegrationEventService(IEventBus eventHub)
    {
        _pendingEvents = [];
        _eventHub = eventHub ?? throw new ArgumentNullException(nameof(eventHub));
    }

    public void AddIntegrationEvent<T>(T @event) where T : IntegrationEvent
    {
        _pendingEvents.Add((typeof(T), @event));
    }

    public async Task PublishIntegrationEventAsync()
    {
        foreach (var (type, @event) in _pendingEvents)
        {
            var method = typeof(IEventBus).GetMethod(nameof(IEventBus.PublishAsync)).MakeGenericMethod(type);
            await (Task)method.Invoke(_eventHub, [@event]);
        }
    }
}
