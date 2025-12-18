namespace Arusha.Template.Domain.Abstractions;

/// <summary>
/// Marker interface for aggregate roots.
/// An aggregate root is the entry point to an aggregate - a cluster of domain objects
/// that are treated as a single unit for data changes.
/// </summary>
public interface IAggregateRoot
{
    /// <summary>
    /// Gets the domain events raised by this aggregate.
    /// </summary>
    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }

    /// <summary>
    /// Clears all domain events from the aggregate.
    /// Called after events have been dispatched/persisted.
    /// </summary>
    void ClearDomainEvents();
}
