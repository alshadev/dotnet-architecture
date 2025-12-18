namespace Arusha.Template.Application.Exceptions;

/// <summary>
/// Exception thrown when request validation fails.
/// </summary>
public sealed class ValidationException : Exception
{
    public ValidationException(IEnumerable<ValidationFailure> failures)
        : base("One or more validation errors occurred.")
    {
        Errors = failures
            .GroupBy(f => f.PropertyName, f => f.ErrorMessage)
            .ToDictionary(g => g.Key, g => g.ToArray());
    }

    /// <summary>
    /// Dictionary of property names to error messages.
    /// </summary>
    public IDictionary<string, string[]> Errors { get; }
}
