namespace Arusha.Template.Application;

/// <summary>
/// Extension methods for registering Application services.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        // MediatR
        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(assembly);

            // Pipeline behaviors (order matters!)
            config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
            config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));
        });

        // FluentValidation
        services.AddValidatorsFromAssembly(assembly);

        return services;
    }
}
