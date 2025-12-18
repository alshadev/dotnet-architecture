namespace Arusha.Template.Application.Results;

/// <summary>
/// Represents the result of a validation operation containing multiple errors.
/// </summary>
public sealed class ValidationResult : Result
{
    private ValidationResult(Error[] errors)
        : base(false, Error.Validation("Validation.Failed", "One or more validation errors occurred."))
    {
        Errors = errors;
    }

    /// <summary>
    /// The collection of validation errors.
    /// </summary>
    public Error[] Errors { get; }

    /// <summary>
    /// Creates a validation result with the specified errors.
    /// </summary>
    public static ValidationResult WithErrors(params Error[] errors) => new(errors);

    /// <summary>
    /// Creates a validation result with the specified errors.
    /// </summary>
    public static ValidationResult WithErrors(IEnumerable<Error> errors) => new(errors.ToArray());
}

/// <summary>
/// Represents the result of a validation operation containing multiple errors with a typed value.
/// </summary>
/// <typeparam name="TValue">The type of the value that would have been returned.</typeparam>
public sealed class ValidationResult<TValue> : Result<TValue>
{
    private ValidationResult(Error[] errors)
        : base(default, false, Error.Validation("Validation.Failed", "One or more validation errors occurred."))
    {
        Errors = errors;
    }

    /// <summary>
    /// The collection of validation errors.
    /// </summary>
    public Error[] Errors { get; }

    /// <summary>
    /// Creates a validation result with the specified errors.
    /// </summary>
    public static ValidationResult<TValue> WithErrors(params Error[] errors) => new(errors);

    /// <summary>
    /// Creates a validation result with the specified errors.
    /// </summary>
    public static ValidationResult<TValue> WithErrors(IEnumerable<Error> errors) => new(errors.ToArray());
}
