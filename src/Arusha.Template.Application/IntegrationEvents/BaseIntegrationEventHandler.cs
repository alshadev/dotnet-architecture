using Arusha.Template.Domain.IntegrationEventLogs;

namespace Arusha.Template.Application.IntegrationEvents;

public abstract class BaseIntegrationEventHandler<TEvent> : IIntegrationEventHandler<TEvent>
    where TEvent : IntegrationEvent
{
    private readonly IIntegrationEventLogService _integrationEventLogService;
    private readonly ILogger _logger;
    private readonly string _eventName;
    private readonly string _eventHandlerName;

    protected BaseIntegrationEventHandler(IIntegrationEventLogService integrationEventLogService, ILogger logger, string eventHandlerName = null)
    {
        _integrationEventLogService = integrationEventLogService ?? throw new ArgumentNullException(nameof(integrationEventLogService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _eventName = typeof(TEvent).Name;
        _eventHandlerName = eventHandlerName ?? $"{_eventName}Handler";
    }

    /// <summary>
    /// Integration event will be handled in within this method
    /// </summary>
    /// <param name="event">integration event object to be handled</param>
    public async Task Handle(TEvent @event)
    {
        await _integrationEventLogService.AddOrSetRetrySubscriberLogAsync(@event, _eventHandlerName);

        try
        {
            _logger.LogInformation("Processing subcription event {EventId} - [{EventName}, {EventHandler}]"
                , @event.Id
                , _eventName
                , _eventHandlerName);

            await ProcessEvent(@event);

            await _integrationEventLogService.MarkAsProcessedAsync(@event.Id, IntegrationEventLogType.Subscriber, _eventHandlerName);

            _logger.LogInformation("Subcription event {EventId} processed - [{EventName}, {EventHandler}]"
                , @event.Id
                , _eventName
                , _eventHandlerName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process subscription event {EventId} - [{EventName}, {EventHandler}]"
                , @event.Id
                , _eventName
                , _eventHandlerName);

            await _integrationEventLogService.MarkAsFailedAsync(@event.Id, IntegrationEventLogType.Subscriber, _eventHandlerName, ex.Message);
        }
    }

    /// <summary>
    /// Abstract method to be implemented by derived event handlers.
    /// </summary>
    /// <param name="event">integration event object to be processed</param>
    protected abstract Task ProcessEvent(TEvent @event);
}

