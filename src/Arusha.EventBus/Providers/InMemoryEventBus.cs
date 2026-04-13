namespace Arusha.EventBus.Providers;

internal sealed class InMemoryEventBus(
    IServiceProvider serviceProvider,
    ILogger<InMemoryEventBus> logger) : IEventBus
{
    private readonly Dictionary<string, List<Type>> _subscriptions = [];

    private static readonly Lazy<Dictionary<string, Type>> _eventNameCache = new(() => AppDomain
        .CurrentDomain
        .GetAssemblies()
        .SelectMany(a => a.GetTypes())
        .Where(t => typeof(IntegrationEvent).IsAssignableFrom(t) && !t.IsAbstract)
        .ToDictionary(t => t.Name, t => t));

    public async Task PublishAsync<T>(T @event) where T : IntegrationEvent
    {
        using var scope = serviceProvider.CreateScope();
        var logService = scope.ServiceProvider.GetRequiredService<IIntegrationEventLogService>();

        await logService.AddOrSetRetryPublisherLogAsync(@event);

        string eventName = @event.GetType().Name;

        try
        {
            if (_subscriptions.TryGetValue(eventName, out var handlerTypes))
            {
                foreach (var handlerType in handlerTypes)
                {
                    var handler = scope.ServiceProvider.GetService(handlerType);
                    if (handler == null) continue;

                    var handleMethod = handlerType.GetMethod("Handle")!;
                    await (Task)handleMethod.Invoke(handler, [@event])!;
                }
            }

            await logService.MarkAsProcessedAsync(@event.Id, IntegrationEventLogType.Publisher, string.Empty);
            logger.LogInformation("[InMemory] Published event {EventId} of name {EventName}", @event.Id, eventName);
        }
        catch (Exception ex)
        {
            await logService.MarkAsFailedAsync(@event.Id, IntegrationEventLogType.Publisher, string.Empty, ex.Message);
            logger.LogError(ex, "[InMemory] Failed to publish event {EventId} of name {EventName}", @event.Id, eventName);
        }
    }

    public Task SubscribeAsync<T, TH>(string serviceName)
        where T : IntegrationEvent
        where TH : IIntegrationEventHandler<T>
    {
        string eventName = typeof(T).Name;

        if (!_subscriptions.TryGetValue(eventName, out var handlers))
        {
            handlers = [];
            _subscriptions[eventName] = handlers;
        }

        var handlerType = typeof(TH);
        if (!handlers.Contains(handlerType))
        {
            handlers.Add(handlerType);
        }

        logger.LogInformation("[InMemory] Subscribed event '{EventName}' with handler '{HandlerName}'", eventName, handlerType.Name);
        return Task.CompletedTask;
    }

    public async Task RepublishAsync(IntegrationEventLog eventLog)
    {
        if (eventLog.Type != IntegrationEventLogType.Publisher) return;

        if (!_eventNameCache.Value.TryGetValue(eventLog.Event, out var eventType)) return;

        if (JsonSerializer.Deserialize(eventLog.Payload, eventType) is IntegrationEvent @event)
        {
            await PublishAsync(@event);
        }
    }

    public async Task ReprocessSubscriptionAsync(IntegrationEventLog eventLog)
    {
        if (eventLog.Type != IntegrationEventLogType.Subscriber) return;

        if (!_eventNameCache.Value.TryGetValue(eventLog.Event, out var eventType)) return;

        if (JsonSerializer.Deserialize(eventLog.Payload, eventType) is not IntegrationEvent integrationEvent) return;

        using var scope = serviceProvider.CreateScope();
        var handlerInterface = typeof(IIntegrationEventHandler<>).MakeGenericType(eventType);
        var handlers = serviceProvider.GetServices(handlerInterface);

        foreach (var handler in handlers)
        {
            if (!handler!.GetType().Name.Equals(eventLog.EventHandler, StringComparison.OrdinalIgnoreCase)) continue;

            var handleMethod = handler.GetType().GetMethod("Handle")!;
            await (Task)handleMethod.Invoke(handler, [integrationEvent])!;
            break;
        }
    }
}
