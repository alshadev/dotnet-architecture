namespace Arusha.EventBus;

public interface IIntegrationEventLogService
{
    Task<IntegrationEventLog> AddOrSetRetryPublisherLogAsync<T>(T @event) where T : IntegrationEvent;
    Task<IntegrationEventLog> AddOrSetRetrySubscriberLogAsync<T>(T @event, string eventHandlerName) where T : IntegrationEvent;
    Task MarkAsProcessedAsync(Guid eventId, IntegrationEventLogType type, string eventHandlerName);
    Task MarkAsFailedAsync(Guid eventId, IntegrationEventLogType type, string eventHandlerName, string remark = null);
}
