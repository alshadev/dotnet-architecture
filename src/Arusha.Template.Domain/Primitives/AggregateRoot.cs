namespace Arusha.Template.Domain.Primitives;

/// <summary>
/// Base class for aggregate roots.
/// An aggregate root is the entry point to an aggregate - a cluster of domain objects
/// that are treated as a single unit for data changes.
/// 
/// Aggregates enforce invariants and raise domain events.
/// </summary>
/// <typeparam name="TId">The type of the aggregate's identifier.</typeparam>
public abstract class AggregateRoot<TId> : Entity<TId>, IAggregateRoot
    where TId : notnull
{
    private readonly List<IDomainEvent> _domainEvents = [];

    protected AggregateRoot(TId id) : base(id)
    {
    }

    /// <summary>
    /// Protected constructor for EF Core.
    /// </summary>
    protected AggregateRoot()
    {
    }

    /// <summary>
    /// Gets the domain events raised by this aggregate.
    /// </summary>
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Raises a domain event.
    /// Events are collected and dispatched during SaveChanges.
    /// </summary>
    /// <param name="domainEvent">The domain event to raise.</param>
    protected void RaiseDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Clears all domain events.
    /// Called after events have been dispatched/persisted.
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
