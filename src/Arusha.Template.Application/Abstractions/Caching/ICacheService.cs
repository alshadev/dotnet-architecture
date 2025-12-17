namespace Arusha.Template.Application.Abstractions.Caching;

/// <summary>
/// Simple cache abstraction that can be backed by in-memory or distributed providers.
/// </summary>
public interface ICacheService
{
    Task<T> GetAsync<T>(string key, CancellationToken cancellationToken = default);

    Task SetAsync<T>(string key, T value, TimeSpan? ttl = null, CancellationToken cancellationToken = default);

    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
}
