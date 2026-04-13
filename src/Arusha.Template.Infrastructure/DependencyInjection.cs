using Arusha.EventBus;
using Arusha.Template.Infrastructure.Persistence.Interceptors;

namespace Arusha.Template.Infrastructure;

/// <summary>
/// Extension methods for registering Infrastructure services.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Database
        services.AddDatabase(configuration);

        // Caching
        services.Configure<CachingOptions>(configuration.GetSection("Cache"));
        services.AddCaching(configuration);

        // Identity & Auth (stores live in ApplicationDbContext)
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

        // Unit of Work (ApplicationDbContext implements IUnitOfWork)
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ApplicationDbContext>());

        // Event Bus
        var eventBusProvider = configuration["EventBus:Provider"] ?? "InMemory";
        var connectionString = configuration.GetConnectionString("Database");
        var dbProvider = configuration["DatabaseProvider"] ?? "SqlServer";
        var handlerAssembly = typeof(Application.DependencyInjection).Assembly;

        services.AddArushaEventBus(options => ConfigureEventBusDb(options, connectionString, dbProvider))
            .UseProvider(eventBusProvider, configuration, handlerAssembly);

        // Idempotency
        services.AddScoped<IIdempotencyService, IdempotencyService>();

        return services;
    }

    private static EventBusBuilder UseProvider(this EventBusBuilder builder, string provider, IConfiguration configuration, System.Reflection.Assembly handlerAssembly)
    {
        return provider.ToLowerInvariant() switch
        {
            "azureservicebus" or "servicebus" => builder.UseAzureServiceBus(
                configuration.GetConnectionString("ServiceBus") ?? "Endpoint=sb://fake.servicebus.windows.net/;SharedAccessKeyName=fake;SharedAccessKey=fake=",
                handlerAssembly),
            "rabbitmq" => builder.UseRabbitMq(
                configuration.GetConnectionString("RabbitMq") ?? "amqp://guest:guest@localhost:5672",
                handlerAssembly),
            _ => builder.UseInMemory(handlerAssembly)
        };
    }

    private static void ConfigureEventBusDb(DbContextOptionsBuilder options, string connectionString, string provider)
    {
        switch (provider.ToLowerInvariant())
        {
            case "postgresql":
            case "postgres":
            case "npgsql":
                options.UseNpgsql(connectionString);
                break;
            case "sqlserver":
            case "mssql":
            default:
                options.UseSqlServer(connectionString);
                break;
        }
    }

    private static IServiceCollection AddCaching(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var cacheProvider = configuration.GetSection("Cache").GetValue<string>("Provider") ?? "InMemory";

        if (string.Equals(cacheProvider, "redis", StringComparison.OrdinalIgnoreCase))
        {
            var redisConnection = configuration.GetSection("Cache").GetValue<string>("ConnectionString")
                ?? configuration.GetConnectionString("Redis");

            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnection;
            });
        }
        else
        {
            services.AddDistributedMemoryCache();
        }

        services.AddSingleton<ICacheService, DistributedCacheService>();

        return services;
    }

    private static IServiceCollection AddDatabase(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Database");
        var provider = configuration["DatabaseProvider"] ?? "SqlServer";

        services.AddScoped<AuditSaveChangesInterceptor>();
        services.AddScoped<DomainEventDispatcher>();

        services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            options.AddInterceptors(sp.GetRequiredService<AuditSaveChangesInterceptor>());
            switch (provider.ToLowerInvariant())
            {
                case "postgresql":
                case "postgres":
                case "npgsql":
                    options.UseNpgsql(connectionString, npgsqlOptions =>
                    {
                        npgsqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                    });
                    break;

                case "sqlserver":
                case "mssql":
                default:
                    options.UseSqlServer(connectionString, sqlOptions =>
                    {
                        sqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                    });
                    break;
            }

            // Enable sensitive data logging in development
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
            {
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            }
        });

        return services;
    }
}
