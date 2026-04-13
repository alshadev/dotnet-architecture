namespace Arusha.EventBus;

internal sealed class IntegrationEventLogService(IntegrationEventDbContext context) : IIntegrationEventLogService
{
    private static readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = false };

    public async Task<IntegrationEventLog> AddOrSetRetryPublisherLogAsync<T>(T @event) where T : IntegrationEvent
        => await AddOrSetRetryLogAsync(@event, IntegrationEventLogType.Publisher, string.Empty);

    public async Task<IntegrationEventLog> AddOrSetRetrySubscriberLogAsync<T>(T @event, string eventHandlerName) where T : IntegrationEvent
        => await AddOrSetRetryLogAsync(@event, IntegrationEventLogType.Subscriber, eventHandlerName);

    public async Task MarkAsProcessedAsync(Guid eventId, IntegrationEventLogType type, string eventHandlerName)
        => await UpdateStatusAsync(eventId, type, IntegrationEventLogStatus.Processed, eventHandlerName);

    public async Task MarkAsFailedAsync(Guid eventId, IntegrationEventLogType type, string eventHandlerName, string remark = null)
        => await UpdateStatusAsync(eventId, type, IntegrationEventLogStatus.Failed, eventHandlerName, remark);

    private async Task<IntegrationEventLog> AddOrSetRetryLogAsync<T>(T @event, IntegrationEventLogType type, string handlerName) where T : IntegrationEvent
    {
        var log = await context.IntegrationEventLogs
            .FirstOrDefaultAsync(l => l.EventId == @event.Id && l.Type == type && l.EventHandler == handlerName);

        if (log != null)
        {
            log.Retry();
        }
        else
        {
            log = new IntegrationEventLog
            {
                EventId = @event.Id,
                Event = @event.GetType().Name,
                EventHandler = handlerName,
                Status = IntegrationEventLogStatus.Processing,
                Payload = JsonSerializer.Serialize(@event, @event.GetType(), _jsonOptions),
                CreatedOnUtc = DateTime.UtcNow,
                LastAttemptOnUtc = DateTime.UtcNow,
                Type = type
            };
            context.IntegrationEventLogs.Add(log);
        }

        await context.SaveChangesAsync();
        return log;
    }

    private async Task UpdateStatusAsync(Guid eventId, IntegrationEventLogType type, IntegrationEventLogStatus status, string handlerName, string remark = null)
    {
        var log = await context.IntegrationEventLogs
            .FirstOrDefaultAsync(l => l.EventId == eventId && l.Type == type && l.EventHandler == handlerName);

        if (log == null) return;

        if (status == IntegrationEventLogStatus.Processed) log.Processed();
        else if (status == IntegrationEventLogStatus.Failed) log.Failed(remark);

        await context.SaveChangesAsync();
    }
}
