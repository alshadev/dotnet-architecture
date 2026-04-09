namespace Arusha.Template.Domain.Abstractions;

/// <summary>
/// Abstraction for obtaining the current UTC date and time.
/// Enables testability of time-dependent domain logic.
/// </summary>
public interface IDateTimeProvider
{
    /// <summary>
    /// Gets the current UTC date and time.
    /// </summary>
    DateTime UtcNow { get; }
}
