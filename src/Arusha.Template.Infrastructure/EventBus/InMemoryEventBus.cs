namespace Arusha.Template.Infrastructure.EventBus;

/// <summary>
/// In-memory event bus implementation.
/// Uses MediatR to dispatch events within the same process.
/// 
/// Suitable for:
/// - Development and testing
/// - Single-instance deployments
/// - When external message broker is not needed
/// 
/// Replace with RabbitMQ/Azure Service Bus for production
/// distributed scenarios.
/// </summary>
public sealed class InMemoryEventBus(
    IMediator mediator,
    ILogger<InMemoryEventBus> logger) : IEventBus
{
    public async Task PublishAsync<T>(T integrationEvent, CancellationToken cancellationToken = default)
        where T : IDomainEvent
    {
        var eventType = integrationEvent.GetType().Name;

        logger.LogInformation(
            "Publishing integration event {EventType} with ID {EventId}",
            eventType,
            integrationEvent.Id);

        try
        {
            // Dispatch via MediatR to all registered handlers
            await mediator.Publish(integrationEvent, cancellationToken);

            logger.LogDebug(
                "Successfully published integration event {EventType}",
                eventType);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Failed to publish integration event {EventType}: {Error}",
                eventType,
                ex.Message);

            throw;
        }
    }
}
