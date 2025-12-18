namespace Arusha.Template.Application.Abstractions.Messaging;

/// <summary>
/// Marker interface for all commands.
/// Commands represent an intent to change the system state.
/// They are handled by a single handler and return a Result.
/// </summary>
public interface ICommand : IRequest<Result>
{
}

/// <summary>
/// Marker interface for commands that return a value.
/// </summary>
/// <typeparam name="TResponse">The type of the response.</typeparam>
public interface ICommand<TResponse> : IRequest<Result<TResponse>>
{
}
