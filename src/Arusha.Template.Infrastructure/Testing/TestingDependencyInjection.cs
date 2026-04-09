namespace Arusha.Template.Infrastructure.Testing;

/// <summary>
/// Extension methods for registering Infrastructure services for integration tests.
/// This keeps test-specific configuration separate from production configuration.
/// </summary>
public static class TestingDependencyInjection
{
    /// <summary>
    /// Adds Infrastructure services configured for integration testing.
    /// Uses SQLite in-memory database to support transactions (required by TransactionBehavior).
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="sqliteConnection">An open SQLite connection for in-memory database.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddInfrastructureForTesting(
        this IServiceCollection services,
        DbConnection sqliteConnection)
    {
        // Database - Use provided SQLite connection
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseSqlite(sqliteConnection);
            options.EnableSensitiveDataLogging();
            options.EnableDetailedErrors();
        });

        // Identity & Auth
        services
            .AddIdentityCore<IdentityUser>()
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>();

        // Context accessors
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddSingleton<IDateTimeProvider, Common.DateTimeProvider>();

        // Repositories
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());

        // Unit of Work
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ApplicationDbContext>());

        // Use in-memory implementations for testing
        services.AddDistributedMemoryCache();
        services.AddSingleton<ICacheService, DistributedCacheService>();

        // Event Bus
        services.AddSingleton<IEventBus, InMemoryEventBus>();

        // Idempotency
        services.AddScoped<IIdempotencyService, IdempotencyService>();

        // Don't register OutboxProcessor for tests (no background processing needed)

        return services;
    }
}
