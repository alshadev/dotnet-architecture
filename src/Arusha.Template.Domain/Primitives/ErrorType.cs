namespace Arusha.Template.Domain.Primitives;

/// <summary>
/// Categorizes domain errors. Multiple error types may map to the same HTTP status code.
/// HTTP status code mapping happens ONLY in the API layer.
/// </summary>
public enum ErrorType
{
    // Success / No Error
    None = 0,

    // Validation errors (400 Bad Request)
    Validation,            // General validation failure
    InvalidInput,          // Invalid input data
    InvalidOperation,      // Operation not allowed in current state
    BusinessRuleViolation, // Domain business rule violated

    // Not found errors (404 Not Found)
    NotFound,              // Entity/resource not found

    // Conflict errors (409 Conflict)
    Conflict,              // Resource conflict (duplicate, version mismatch)
    AlreadyExists,         // Entity already exists
    ConcurrencyConflict,   // Optimistic concurrency failure

    // Authorization errors (403 Forbidden)
    Forbidden,             // User lacks permission
    InsufficientPermissions,

    // Authentication errors (401 Unauthorized)
    Unauthorized,          // Not authenticated
    TokenExpired,
    InvalidCredentials,

    // Server errors (500 Internal Server Error)
    Failure,               // Unexpected failure
    ServiceUnavailable,    // External service unavailable

    // Unprocessable (422 Unprocessable Entity)
    Unprocessable          // Request understood but cannot be processed
}
