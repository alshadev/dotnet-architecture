namespace Arusha.Template.Domain.Primitives;
/// <summary>
/// Represents a domain error without HTTP/transport concerns.
/// Use factory methods to create specific error types.
/// </summary>
/// <param name="Type">The category of the error.</param>
/// <param name="Code">A unique code identifying the error (e.g., "Order.InvalidStatus").</param>
/// <param name="Message">A human-readable description of the error.</param>
public sealed record Error(ErrorType Type, string Code, string Message)
{
    /// <summary>
    /// Represents no error (success).
    /// </summary>
    public static readonly Error None = new(ErrorType.None, string.Empty, string.Empty);

    /// <summary>
    /// Represents a null value error.
    /// </summary>
    public static readonly Error NullValue = new(ErrorType.Validation, "Error.NullValue", "A null value was provided.");

    #region Factory Methods - Validation (400)

    /// <summary>
    /// Creates a general validation error.
    /// </summary>
    public static Error Validation(string code, string message)
        => new(ErrorType.Validation, code, message);

    /// <summary>
    /// Creates an invalid input error.
    /// </summary>
    public static Error InvalidInput(string code, string message)
        => new(ErrorType.InvalidInput, code, message);

    /// <summary>
    /// Creates an invalid operation error (operation not allowed in current state).
    /// </summary>
    public static Error InvalidOperation(string code, string message)
        => new(ErrorType.InvalidOperation, code, message);

    /// <summary>
    /// Creates a business rule violation error.
    /// </summary>
    public static Error BusinessRuleViolation(string code, string message)
        => new(ErrorType.BusinessRuleViolation, code, message);

    #endregion

    #region Factory Methods - Not Found (404)

    /// <summary>
    /// Creates a not found error.
    /// </summary>
    public static Error NotFound(string code, string message)
        => new(ErrorType.NotFound, code, message);

    #endregion

    #region Factory Methods - Conflict (409)

    /// <summary>
    /// Creates a conflict error.
    /// </summary>
    public static Error Conflict(string code, string message)
        => new(ErrorType.Conflict, code, message);

    /// <summary>
    /// Creates an already exists error.
    /// </summary>
    public static Error AlreadyExists(string code, string message)
        => new(ErrorType.AlreadyExists, code, message);

    /// <summary>
    /// Creates a concurrency conflict error.
    /// </summary>
    public static Error ConcurrencyConflict(string code, string message)
        => new(ErrorType.ConcurrencyConflict, code, message);

    #endregion

    #region Factory Methods - Authorization (403)

    /// <summary>
    /// Creates a forbidden error.
    /// </summary>
    public static Error Forbidden(string code, string message)
        => new(ErrorType.Forbidden, code, message);

    /// <summary>
    /// Creates an insufficient permissions error.
    /// </summary>
    public static Error InsufficientPermissions(string code, string message)
        => new(ErrorType.InsufficientPermissions, code, message);

    #endregion

    #region Factory Methods - Authentication (401)

    /// <summary>
    /// Creates an unauthorized error.
    /// </summary>
    public static Error Unauthorized(string code, string message)
        => new(ErrorType.Unauthorized, code, message);

    /// <summary>
    /// Creates a token expired error.
    /// </summary>
    public static Error TokenExpired(string code, string message)
        => new(ErrorType.TokenExpired, code, message);

    /// <summary>
    /// Creates an invalid credentials error.
    /// </summary>
    public static Error InvalidCredentials(string code, string message)
        => new(ErrorType.InvalidCredentials, code, message);

    #endregion

    #region Factory Methods - Server Errors (500/503)

    /// <summary>
    /// Creates an unexpected failure error.
    /// </summary>
    public static Error Failure(string code, string message)
        => new(ErrorType.Failure, code, message);

    /// <summary>
    /// Creates a service unavailable error.
    /// </summary>
    public static Error ServiceUnavailable(string code, string message)
        => new(ErrorType.ServiceUnavailable, code, message);

    #endregion

    #region Factory Methods - Unprocessable (422)

    /// <summary>
    /// Creates an unprocessable entity error.
    /// </summary>
    public static Error Unprocessable(string code, string message)
        => new(ErrorType.Unprocessable, code, message);

    #endregion

    public override string ToString() => $"{Type}: [{Code}] {Message}";
}