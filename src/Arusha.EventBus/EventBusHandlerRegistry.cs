namespace Arusha.EventBus;

internal sealed class EventBusHandlerRegistry
{
    public List<(Type EventType, Type HandlerType)> Handlers { get; } = [];
}
