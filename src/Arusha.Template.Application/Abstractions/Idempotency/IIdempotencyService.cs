namespace Arusha.Template.Application.Abstractions.Idempotency;

/// <summary>
/// Service for checking and recording idempotent requests.
/// Prevents duplicate processing in distributed systems.
/// </summary>
public interface IIdempotencyService
{
    /// <summary>
    /// Checks if a request with the given ID has already been processed.
    /// </summary>
    Task<bool> RequestExistsAsync(Guid requestId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a record for the request ID.
    /// </summary>
    Task CreateRequestAsync(Guid requestId, string name, CancellationToken cancellationToken = default);
}
