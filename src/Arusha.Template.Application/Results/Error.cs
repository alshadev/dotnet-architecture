namespace Arusha.Template.Application.Results;

/// <summary>
/// Represents an error that occurred during an operation.
/// </summary>
/// <param name="Code">A unique code identifying the error type.</param>
/// <param name="Message">A human-readable description of the error.</param>
public sealed record Error(string Code, string Message)
{
    /// <summary>
    /// Represents no error.
    /// </summary>
    public static readonly Error None = new(string.Empty, string.Empty);

    /// <summary>
    /// Represents a null value error.
    /// </summary>
    public static readonly Error NullValue = new("Error.NullValue", "A null value was provided.");

    /// <summary>
    /// Represents a validation error.
    /// </summary>
    public static Error Validation(string code, string message) => new(code, message);

    /// <summary>
    /// Represents a not found error.
    /// </summary>
    public static Error NotFound(string code, string message) => new(code, message);

    /// <summary>
    /// Represents a conflict error (duplicate, already exists, etc.).
    /// </summary>
    public static Error Conflict(string code, string message) => new(code, message);

    /// <summary>
    /// Represents an internal/unexpected error.
    /// </summary>
    public static Error Failure(string code, string message) => new(code, message);

    /// <summary>
    /// Represents an unauthorized access error.
    /// </summary>
    public static Error Unauthorized(string code, string message) => new(code, message);

    /// <summary>
    /// Represents a forbidden access error.
    /// </summary>
    public static Error Forbidden(string code, string message) => new(code, message);

    public override string ToString() => $"{Code}: {Message}";
}
