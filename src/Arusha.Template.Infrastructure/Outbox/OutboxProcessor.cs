namespace Arusha.Template.Infrastructure.Outbox;

/// <summary>
/// Background service that processes outbox messages.
/// Polls for unprocessed messages and publishes them to the event bus.
/// </summary>
public sealed class OutboxProcessor : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IEventBus _eventBus;
    private readonly ILogger<OutboxProcessor> _logger;
    private readonly TimeSpan _pollingInterval = TimeSpan.FromSeconds(10);
    private readonly int _batchSize = 20;
    private readonly int _maxRetries = 3;

    public OutboxProcessor(
        IServiceProvider serviceProvider,
        IEventBus eventBus,
        ILogger<OutboxProcessor> logger)
    {
        _serviceProvider = serviceProvider;
        _eventBus = eventBus;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Outbox processor started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessOutboxMessagesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing outbox messages");
            }

            await Task.Delay(_pollingInterval, stoppingToken);
        }

        _logger.LogInformation("Outbox processor stopped");
    }

    private async Task ProcessOutboxMessagesAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var messages = await dbContext.OutboxMessages
            .Where(m => m.ProcessedOnUtc == default && m.RetryCount < _maxRetries)
            .OrderBy(m => m.OccurredOnUtc)
            .Take(_batchSize)
            .ToListAsync(cancellationToken);

        if (messages.Count == 0)
        {
            return;
        }

        _logger.LogDebug("Processing {Count} outbox messages", messages.Count);

        foreach (var message in messages)
        {
            try
            {
                var eventType = Type.GetType(message.Type);
                if (eventType is null)
                {
                    _logger.LogWarning(
                        "Could not resolve type {Type} for outbox message {MessageId}",
                        message.Type,
                        message.Id);

                    message.MarkAsFailed($"Could not resolve type: {message.Type}");
                    continue;
                }

                var domainEvent = JsonSerializer.Deserialize(message.Content, eventType) as IDomainEvent;
                if (domainEvent is null)
                {
                    _logger.LogWarning(
                        "Could not deserialize outbox message {MessageId}",
                        message.Id);

                    message.MarkAsFailed("Could not deserialize message content");
                    continue;
                }

                await _eventBus.PublishAsync(domainEvent, cancellationToken);

                message.MarkAsProcessed();

                _logger.LogDebug(
                    "Published outbox message {MessageId} of type {EventType}",
                    message.Id,
                    eventType.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to process outbox message {MessageId}: {Error}",
                    message.Id,
                    ex.Message);

                message.MarkAsFailed(ex.Message);
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
