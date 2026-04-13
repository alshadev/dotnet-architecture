namespace Arusha.EventBus;

internal sealed class IntegrationEventService(IEventBus eventBus) : IIntegrationEventService
{
    private readonly List<(Type Type, IntegrationEvent Event)> _pendingEvents = [];

    public void AddIntegrationEvent<T>(T @event) where T : IntegrationEvent
    {
        _pendingEvents.Add((typeof(T), @event));
    }

    public async Task PublishIntegrationEventAsync()
    {
        var publishMethod = typeof(IEventBus).GetMethod(nameof(IEventBus.PublishAsync))!;

        foreach (var (type, @event) in _pendingEvents)
        {
            var genericMethod = publishMethod.MakeGenericMethod(type);
            await (Task)genericMethod.Invoke(eventBus, [@event])!;
        }

        _pendingEvents.Clear();
    }
}
