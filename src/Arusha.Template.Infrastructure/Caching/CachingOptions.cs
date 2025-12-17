namespace Arusha.Template.Infrastructure.Caching;

public sealed class CachingOptions
{
    public string Provider { get; set; } = "InMemory"; // InMemory or Redis
    public string ConnectionString { get; set; }
    public int DefaultTtlSeconds { get; set; } = 300;
}
