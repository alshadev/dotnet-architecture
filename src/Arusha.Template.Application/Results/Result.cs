namespace Arusha.Template.Application.Results;

/// <summary>
/// Represents the result of an operation that can either succeed or fail.
/// Use this for operations that don't return a value.
/// </summary>
public class Result
{
    protected Result(bool isSuccess, Error error)
    {
        if (isSuccess && error != Error.None)
            throw new InvalidOperationException("A successful result cannot have an error.");

        if (!isSuccess && error == Error.None)
            throw new InvalidOperationException("A failed result must have an error.");

        IsSuccess = isSuccess;
        Error = error;
    }

    /// <summary>
    /// Indicates whether the operation was successful.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Indicates whether the operation failed.
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// The error that caused the operation to fail.
    /// </summary>
    public Error Error { get; }

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    public static Result Success() => new(true, Error.None);

    /// <summary>
    /// Creates a successful result with a value.
    /// </summary>
    public static Result<TValue> Success<TValue>(TValue value) => new(value, true, Error.None);

    /// <summary>
    /// Creates a failed result.
    /// </summary>
    public static Result Failure(Error error) => new(false, error);

    /// <summary>
    /// Creates a failed result with a type.
    /// </summary>
    public static Result<TValue> Failure<TValue>(Error error) => new(default, false, error);

    /// <summary>
    /// Creates a result based on a condition.
    /// </summary>
    public static Result Create(bool condition, Error error) =>
        condition ? Success() : Failure(error);

    /// <summary>
    /// Creates a result from a nullable value.
    /// </summary>
    public static Result<TValue> Create<TValue>(TValue value, Error error) =>
        value is not null ? Success(value) : Failure<TValue>(error);

    /// <summary>
    /// Implicitly converts an error to a failed result.
    /// </summary>
    public static implicit operator Result(Error error) => Failure(error);
}

/// <summary>
/// Represents the result of an operation that returns a value.
/// </summary>
/// <typeparam name="TValue">The type of the returned value.</typeparam>
public class Result<TValue> : Result
{
    private readonly TValue _value;

    internal Result(TValue value, bool isSuccess, Error error)
        : base(isSuccess, error)
    {
        _value = value;
    }

    /// <summary>
    /// Gets the value of a successful operation.
    /// Throws if the result is a failure.
    /// </summary>
    public TValue Value =>
        IsSuccess
            ? _value!
            : throw new InvalidOperationException("Cannot access value of a failed result.");

    /// <summary>
    /// Implicitly converts a value to a successful result.
    /// </summary>
    public static implicit operator Result<TValue>(TValue value) =>
        value is not null ? Success(value) : Failure<TValue>(Error.NullValue);

    /// <summary>
    /// Implicitly converts an error to a failed result.
    /// </summary>
    public static implicit operator Result<TValue>(Error error) => Failure<TValue>(error);
}
