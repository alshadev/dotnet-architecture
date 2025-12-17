namespace Arusha.Template.Domain.Primitives;

/// <summary>
/// Base interface for strongly typed identifiers.
/// Provides type safety by wrapping primitive ID values.
/// </summary>
/// <typeparam name="TValue">The underlying value type (typically Guid).</typeparam>
public interface IStronglyTypedId<TValue>
    where TValue : notnull
{
    /// <summary>
    /// Gets the underlying value of the identifier.
    /// </summary>
    TValue Value { get; }
}
