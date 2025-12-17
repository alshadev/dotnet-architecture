namespace Arusha.Template.Application.Abstractions.Messaging;

/// <summary>
/// Handler for domain events.
/// Domain event handlers react to things that have happened in the domain.
/// Multiple handlers can respond to the same event.
/// </summary>
/// <typeparam name="TDomainEvent">The type of the domain event.</typeparam>
public interface IDomainEventHandler<in TDomainEvent> : INotificationHandler<TDomainEvent>
    where TDomainEvent : IDomainEvent
{
}
