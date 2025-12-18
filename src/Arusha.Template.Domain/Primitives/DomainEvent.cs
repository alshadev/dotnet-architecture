namespace Arusha.Template.Domain.Primitives;

/// <summary>
/// Base record for domain events.
/// Domain events represent something meaningful that happened in the domain.
/// </summary>
public abstract record DomainEvent : IDomainEvent
{
    /// <summary>
    /// Unique identifier for this event instance.
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// UTC timestamp when the event occurred.
    /// </summary>
    public DateTime OccurredOnUtc { get; init; } = DateTime.UtcNow;
}
