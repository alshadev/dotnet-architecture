using Arusha.EventBus.Providers;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using RabbitMQ.Client;

namespace Arusha.EventBus;

public static class DependencyInjection
{
    /// <summary>
    /// Registers core event bus services (IntegrationEventDbContext, log service, event service).
    /// Chain with UseAzureServiceBus, UseRabbitMq, or UseInMemory to select a provider.
    /// </summary>
    public static EventBusBuilder AddArushaEventBus(
        this IServiceCollection services,
        Action<DbContextOptionsBuilder> configureDbContext)
    {
        services.AddDbContext<IntegrationEventDbContext>(configureDbContext);
        services.AddScoped<IIntegrationEventLogService, IntegrationEventLogService>();
        services.AddScoped<IIntegrationEventService, IntegrationEventService>();
        services.AddSingleton<EventBusHandlerRegistry>();

        return new EventBusBuilder(services);
    }

    /// <summary>
    /// Auto-subscribes all registered event handlers at application startup.
    /// Call this after the application is built (e.g., after builder.Build()).
    /// </summary>
    public static async Task UseArushaEventBusAsync(this IHost host, string serviceName)
    {
        var registry = host.Services.GetRequiredService<EventBusHandlerRegistry>();
        var eventBus = host.Services.GetRequiredService<IEventBus>();
        var logger = host.Services.GetRequiredService<ILogger<IEventBus>>();

        var subscribeMethod = typeof(IEventBus).GetMethod(nameof(IEventBus.SubscribeAsync))!;

        foreach (var (eventType, handlerType) in registry.Handlers)
        {
            try
            {
                var genericMethod = subscribeMethod.MakeGenericMethod(eventType, handlerType);
                await (Task)genericMethod.Invoke(eventBus, [serviceName])!;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to subscribe event '{EventType}' with handler '{HandlerType}'",
                    eventType.Name, handlerType.Name);
                throw;
            }
        }
    }
}

public sealed class EventBusBuilder(IServiceCollection services)
{
    /// <summary>
    /// Use Azure Service Bus as the event bus provider.
    /// </summary>
    public EventBusBuilder UseAzureServiceBus(string connectionString, Assembly handlerAssembly)
    {
        services.AddSingleton<IEventBus, AzureServiceBusEventBus>();
        services.AddSingleton(new ServiceBusClient(connectionString));
        services.AddSingleton(new ServiceBusAdministrationClient(connectionString));

        RegisterHandlers(services, handlerAssembly);
        return this;
    }

    /// <summary>
    /// Use RabbitMQ as the event bus provider.
    /// </summary>
    public EventBusBuilder UseRabbitMq(string connectionString, Assembly handlerAssembly)
    {
        services.AddSingleton<IEventBus, RabbitMqEventBus>();
        services.AddSingleton(sp =>
        {
            var factory = new ConnectionFactory { Uri = new Uri(connectionString) };
            return factory.CreateConnectionAsync().GetAwaiter().GetResult();
        });

        RegisterHandlers(services, handlerAssembly);
        return this;
    }

    /// <summary>
    /// Use in-memory event bus (for development and testing).
    /// </summary>
    public EventBusBuilder UseInMemory(Assembly handlerAssembly)
    {
        services.AddSingleton<IEventBus, InMemoryEventBus>();

        RegisterHandlers(services, handlerAssembly);
        return this;
    }

    private static void RegisterHandlers(IServiceCollection services, Assembly handlerAssembly)
    {
        var handlerInterfaceType = typeof(IIntegrationEventHandler<>);

        var handlerTypes = handlerAssembly
            .GetTypes()
            .Where(type => type.GetInterfaces()
                .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == handlerInterfaceType)
                && !type.IsAbstract && !type.IsInterface);

        // Build registry for auto-subscription
        using var tempProvider = services.BuildServiceProvider();
        var registry = new EventBusHandlerRegistry();

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

                // Extract TEvent from IIntegrationEventHandler<TEvent>
                var eventType = implementedInterface.GetGenericArguments()[0];
                registry.Handlers.Add((eventType, handler));
            }
        }

        // Replace any previously registered registry
        services.AddSingleton(registry);
    }
}
