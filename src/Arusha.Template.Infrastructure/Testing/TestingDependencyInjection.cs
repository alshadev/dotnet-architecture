using Arusha.EventBus;
using Arusha.Template.Infrastructure.Persistence.Interceptors;

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
    public static IServiceCollection AddInfrastructureForTesting(
        this IServiceCollection services,
        DbConnection sqliteConnection)
    {
        // Database - Use provided SQLite connection
        services.AddScoped<AuditSaveChangesInterceptor>();
        services.AddScoped<DomainEventDispatcher>();

        services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            options.AddInterceptors(sp.GetRequiredService<AuditSaveChangesInterceptor>());
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

        // Event Bus (InMemory for testing)
        var handlerAssembly = typeof(Application.DependencyInjection).Assembly;
        services.AddArushaEventBus(options => options.UseSqlite(sqliteConnection))
            .UseInMemory(handlerAssembly);

        // Idempotency
        services.AddScoped<IIdempotencyService, IdempotencyService>();

        return services;
    }
}
