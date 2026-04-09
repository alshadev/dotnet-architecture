using Arusha.Template.Domain.IntegrationEventLogs;

namespace Arusha.Template.Infrastructure.Integration;

public class IntegrationEventLogService<TDbContext>(TDbContext context) : IIntegrationEventLogService where TDbContext : DbContext
{
    private readonly TDbContext _context = context ?? throw new ArgumentNullException(nameof(context));
    private static readonly JsonSerializerOptions _indentedOptions = new() { WriteIndented = false };

    public async Task<IntegrationEventLog> AddOrSetRetryPublisherLogAsync<T>(T @event) where T : IntegrationEvent
        => await AddOrSetRetryLogAsync<T>(@event, IntegrationEventLogType.Publisher);
    public async Task<IntegrationEventLog> AddOrSetRetrySubscriberLogAsync<T>(T @event, string handlerName) where T : IntegrationEvent
        => await AddOrSetRetryLogAsync<T>(@event, IntegrationEventLogType.Publisher, handlerName);

    public async Task MarkAsFailedAsync(Guid eventId, IntegrationEventLogType type, string eventHandlerName, string remark = null)
        => await UpdateStatusAsync(eventId, type, IntegrationEventLogStatus.Failed, eventHandlerName, remark);
    public async Task MarkAsProcessedAsync(Guid eventId, IntegrationEventLogType type, string eventHandlerName)
        => await UpdateStatusAsync(eventId, type, IntegrationEventLogStatus.Processed, eventHandlerName);

    private async Task<IntegrationEventLog> AddOrSetRetryLogAsync<T>(T @event, IntegrationEventLogType type, string handlerName = null) where T : IntegrationEvent
    {
        var log = await GetByPrimaryKeyAsync(@event.Id, type, string.Empty);

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
                Payload = JsonSerializer.Serialize(@event, @event.GetType(), _indentedOptions),
                CreatedOnUtc = DateTime.UtcNow,
                LastAttemptOnUtc = DateTime.UtcNow,
                Type = type
            };
            _context.Add(log);
        }

        await _context.SaveChangesAsync();
        return log;
    }
    private async Task UpdateStatusAsync(Guid eventId, IntegrationEventLogType type, IntegrationEventLogStatus status, string handlerTypeName, string remark = null)
    {
        var eventLogEntry = await GetByPrimaryKeyAsync(eventId, type, handlerTypeName);

        if (status == IntegrationEventLogStatus.Processed) eventLogEntry.Processed();
        else if (status == IntegrationEventLogStatus.Failed) eventLogEntry.Failed(remark);

        await _context.SaveChangesAsync();
    }
    private async Task<IntegrationEventLog> GetByPrimaryKeyAsync(Guid eventId, IntegrationEventLogType type, string handlerTypeName)
    {
        return await _context
            .Set<IntegrationEventLog>()
            .FirstOrDefaultAsync(l => l.EventId == eventId && l.Type == type && l.EventHandler == handlerTypeName);
    }
}
