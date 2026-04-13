namespace Arusha.EventBus;

public abstract class BaseIntegrationEventHandler<TEvent>(
    IIntegrationEventLogService integrationEventLogService,
    ILogger logger,
    string eventHandlerName = null) : IIntegrationEventHandler<TEvent>
    where TEvent : IntegrationEvent
{
    private readonly string _eventName = typeof(TEvent).Name;
    private readonly string _eventHandlerName = eventHandlerName ?? $"{typeof(TEvent).Name}Handler";

    public async Task Handle(TEvent @event)
    {
        await integrationEventLogService.AddOrSetRetrySubscriberLogAsync(@event, _eventHandlerName);

        try
        {
            logger.LogInformation("Processing subscription event {EventId} - [{EventName}, {EventHandler}]",
                @event.Id, _eventName, _eventHandlerName);

            await ProcessEvent(@event);

            await integrationEventLogService.MarkAsProcessedAsync(@event.Id, IntegrationEventLogType.Subscriber, _eventHandlerName);

            logger.LogInformation("Subscription event {EventId} processed - [{EventName}, {EventHandler}]",
                @event.Id, _eventName, _eventHandlerName);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to process subscription event {EventId} - [{EventName}, {EventHandler}]",
                @event.Id, _eventName, _eventHandlerName);

            await integrationEventLogService.MarkAsFailedAsync(@event.Id, IntegrationEventLogType.Subscriber, _eventHandlerName, ex.Message);
        }
    }

    protected abstract Task ProcessEvent(TEvent @event);
}
