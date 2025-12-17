namespace Arusha.Template.Application.Abstractions.Messaging;

/// <summary>
/// Marker interface for all queries.
/// Queries represent a request to read data from the system.
/// They should not modify state (CQRS principle).
/// </summary>
/// <typeparam name="TResponse">The type of the query response.</typeparam>
public interface IQuery<TResponse> : IRequest<Result<TResponse>>
{
}
