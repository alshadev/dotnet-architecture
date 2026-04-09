namespace Arusha.Template.Infrastructure.Idempotency;

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
