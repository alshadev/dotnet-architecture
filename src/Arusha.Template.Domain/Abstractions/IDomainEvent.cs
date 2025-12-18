namespace Arusha.Template.Domain.Abstractions;

/// <summary>
/// Marker interface for domain events.
/// Domain events represent something meaningful that happened in the domain.
/// </summary>
public interface IDomainEvent : INotification
{
    /// <summary>
    /// Unique identifier for the event instance.
    /// </summary>
    Guid Id { get; }

    /// <summary>
    /// UTC timestamp when the event occurred.
    /// </summary>
    DateTime OccurredOnUtc { get; }
}
