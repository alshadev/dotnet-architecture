namespace Arusha.Template.Infrastructure.Persistence;

/// <summary>
/// Dispatches domain events from aggregate roots via MediatR.
/// Supports cascading: if a handler raises new domain events, they are dispatched in subsequent passes.
/// </summary>
public sealed class DomainEventDispatcher(IMediator mediator, ILogger<DomainEventDispatcher> logger)
{
    /// <summary>
    /// Dispatches all pending domain events from tracked aggregates.
    /// Loops until no new events are raised, handling cascading domain events.
    /// </summary>
    public async Task DispatchAndClearAsync(DbContext context, CancellationToken cancellationToken)
    {
        const int maxIterations = 10;
        var iteration = 0;

        while (iteration++ < maxIterations)
        {
            var domainEvents = context.ChangeTracker.Entries<IAggregateRoot>()
                .SelectMany(e => e.Entity.DomainEvents)
                .ToList();

            if (domainEvents.Count == 0) break;

            // Clear before dispatch so new events raised by handlers are captured next iteration
            foreach (var entry in context.ChangeTracker.Entries<IAggregateRoot>())
            {
                entry.Entity.ClearDomainEvents();
            }

            foreach (var domainEvent in domainEvents)
            {
                logger.LogDebug("Dispatching domain event {EventType}", domainEvent.GetType().Name);
                await mediator.Publish(domainEvent, cancellationToken);
            }
        }

        if (iteration > maxIterations)
        {
            logger.LogWarning("Domain event dispatch exceeded {MaxIterations} iterations — possible infinite loop", maxIterations);
        }
    }
}
