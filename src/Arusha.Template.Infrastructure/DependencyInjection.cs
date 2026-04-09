using System.Reflection;
using Arusha.Template.Application.IntegrationEvents.Events;
using Arusha.Template.Domain.IntegrationEventLogs;
using Arusha.Template.Infrastructure.Integration;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using Microsoft.AspNetCore.Builder;

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

        // Event Bus (choose one Azure, RabbitMQ, or In-Memory)
        services.AddAzureServiceBusServices<ApplicationDbContext>("Endpoint=sb://fake.servicebus.windows.net/;SharedAccessKeyName=fake;SharedAccessKey=fake="
            , typeof(Application.DependencyInjection).Assembly);

        // Idempotency
        services.AddScoped<IIdempotencyService, IdempotencyService>();

        // Outbox Processor
        services.AddHostedService<OutboxProcessor>();

        return services;
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

        services.AddDbContext<ApplicationDbContext>(options =>
        {
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

    /// <summary>
    /// Register dependencies of Event Bus that needed using Azure Service Bus.
    /// </summary>
    /// <typeparam name="TTopicRegistry">class that used to mapping each event type and the topic used/typeparam>
    /// <typeparam name="TDbContext">DbContext that used to maintain Integration Event Log</typeparam>
    /// <param name="services"></param>
    /// <param name="serviceBusConnectionString">Connection string that used for Service Bus Client</param>
    /// <param name="assembly">Class that used to identified where the place of IntegrationEventHandler lists</param>
    /// <returns>Service Collections</returns>
    public static IServiceCollection AddAzureServiceBusServices<TDbContext>(this IServiceCollection services, string serviceBusConnectionString, Assembly assembly)
        where TDbContext : DbContext
    {
        services.AddSingleton<IEventBus, AzureServiceBusEventBus>();
        services.AddSingleton(new ServiceBusClient(serviceBusConnectionString));
        services.AddSingleton(new ServiceBusAdministrationClient(serviceBusConnectionString));

        services.RegisterEventBus<TDbContext>(assembly);
        return services;
    }

    /// <summary>
    /// Register dependencies of Event Bus that needed.
    /// </summary>
    /// <typeparam name="TTopicRegistry">class that used to mapping each event type and the topic used/typeparam>
    /// <typeparam name="TDbContext">DbContext that used to maintain Integration Event Log</typeparam>
    /// <param name="services"></param>
    /// <param name="serviceBusConnectionString">Connection string that used for Service Bus Client</param>
    /// <param name="assembly">Class that used to identified where the place of IntegrationEventHandler lists</param>
    /// <returns>Service Collections</returns>
    private static IServiceCollection RegisterEventBus<TDbContext>(this IServiceCollection services, Assembly assembly)
        where TDbContext : DbContext
    {
        services.AddScoped<IIntegrationEventLogService, IntegrationEventLogService<TDbContext>>();
        services.AddScoped<IIntegrationEventService, IntegrationEventService>();

        var handlerInterfaceType = typeof(IIntegrationEventHandler<>);

        var handlerTypes = assembly
            .GetTypes()
            .Where(type => type.GetInterfaces()
                .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == handlerInterfaceType)
                && !type.IsAbstract && !type.IsInterface);

        foreach (var handler in handlerTypes)
        {
            // Register concrete type (for GetService(concreteType))
            services.AddTransient(handler);

            // Register interface type (for GetServices<IIntegrationEventHandler<T>>)
            var implementedInterfaces = handler.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == handlerInterfaceType);

            foreach (var implementedInterface in implementedInterfaces)
            {
                services.AddTransient(implementedInterface, handler);
            }
        }

        return services;
    }

    public static void UseIntegrationEventLog(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<IntegrationEventLog>(builder =>
        {
            builder.ToTable("IntegrationEventLogs", "log");

            builder.HasKey(iel => new { iel.EventId, iel.Type, iel.EventHandler });

            builder
                .Property(iel => iel.Type)
                .HasMaxLength(20)
                .IsRequired();

            builder
                .Property(iel => iel.Status)
                .HasMaxLength(20)
                .IsRequired();

            builder
                .Property(iel => iel.Event)
                .HasMaxLength(255)
                .IsRequired();

            builder
                .Property(iel => iel.EventHandler)
                .HasMaxLength(255)
                .HasDefaultValue(string.Empty)
                .IsRequired();

            builder
                .Property(iel => iel.Payload)
                .IsRequired();

            builder.HasIndex(iel => iel.Type);
            builder.HasIndex(iel => iel.Status);
            builder.HasIndex(iel => iel.Event);
            builder.HasIndex(iel => iel.EventHandler);
            builder.HasIndex(iel => iel.CreatedOnUtc);
            builder.HasIndex(iel => iel.LastAttemptOnUtc);
        });
    }

    public static void SubscribeEvent(this WebApplication app, string serviceName)
    {
        var scope = app.Services.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<WebApplication>>();
        var eventHub = scope.ServiceProvider.GetRequiredService<IEventBus>();

        try
        {
            Task.Run(async () =>
            {
                await eventHub.SubscribeAsync<OrderCreatedIntegrationEvent, OrderCreatedIntegrationEventHandler>(serviceName);
            }).Wait();
        }
        catch (Exception ex)
        {
            logger.LogError("An error occured while register subscriber events");
            logger.LogError(ex.ToString());

            throw;
        }
    }
}
