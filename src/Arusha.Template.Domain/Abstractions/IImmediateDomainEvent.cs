namespace Arusha.Template.Domain.Abstractions;

/// <summary>
/// Marker interface for immediate domain events.
/// These events are dispatched synchronously BEFORE SaveChanges,
/// allowing handlers to modify the same transaction.
/// Use for: audit logs, derived data, same-context side effects.
/// </summary>
public interface IImmediateDomainEvent : IDomainEvent
{
}
