namespace Arusha.Template.Application.Abstractions.Messaging;

/// <summary>
/// Handler for queries.
/// Queries should only read data and never modify state.
/// </summary>
/// <typeparam name="TQuery">The type of the query.</typeparam>
/// <typeparam name="TResponse">The type of the response.</typeparam>
public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>
{
}
