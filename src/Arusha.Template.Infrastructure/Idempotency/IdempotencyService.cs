namespace Arusha.Template.Infrastructure.Idempotency;

/// <summary>
/// Service for checking and recording idempotent requests.
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

/// <summary>
/// EF Core implementation of idempotency service.
/// </summary>
public sealed class IdempotencyService(ApplicationDbContext context) : IIdempotencyService
{
    public async Task<bool> RequestExistsAsync(Guid requestId, CancellationToken cancellationToken = default)
    {
        return await context.IdempotentRequests
            .AnyAsync(r => r.Id == requestId, cancellationToken);
    }

    public async Task CreateRequestAsync(
        Guid requestId,
        string name,
        CancellationToken cancellationToken = default)
    {
        var request = IdempotentRequest.Create(requestId, name);
        context.IdempotentRequests.Add(request);
        // Note: SaveChanges is called by the caller/TransactionBehavior
    }
}
