namespace Arusha.Template.Infrastructure.Caching;

/// <summary>
/// Cache service backed by IDistributedCache so it can use in-memory or Redis transparently.
/// </summary>
internal sealed class DistributedCacheService(IDistributedCache cache, ILogger<DistributedCacheService> logger) : ICacheService
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<T> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        var bytes = await cache.GetAsync(key, cancellationToken);
        if (bytes is null || bytes.Length == 0)
        {
            return default;
        }

        try
        {
            return JsonSerializer.Deserialize<T>(bytes, SerializerOptions);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to deserialize cache entry for {Key}", key);
            await cache.RemoveAsync(key, cancellationToken);
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? ttl = null, CancellationToken cancellationToken = default)
    {
        var bytes = JsonSerializer.SerializeToUtf8Bytes(value, SerializerOptions);
        var options = new DistributedCacheEntryOptions();
        if (ttl.HasValue)
        {
            options.SetAbsoluteExpiration(ttl.Value);
        }

        await cache.SetAsync(key, bytes, options, cancellationToken);
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        await cache.RemoveAsync(key, cancellationToken);
    }
}
