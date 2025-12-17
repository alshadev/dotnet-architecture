namespace Arusha.Template.Application.Abstractions.Security;

/// <summary>
/// Provides information about the currently authenticated user.
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// Current user identifier or null when anonymous/system.
    /// </summary>
    string UserId { get; }

    /// <summary>
    /// Correlation identifier for the current request.
    /// </summary>
    string CorrelationId { get; }
}
