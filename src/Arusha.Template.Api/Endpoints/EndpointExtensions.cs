namespace Arusha.Template.Api.Endpoints;

/// <summary>
/// Extension methods for endpoint registration.
/// </summary>
public static class EndpointExtensions
{
    /// <summary>
    /// Registers all endpoint modules from the assembly.
    /// </summary>
    public static IEndpointRouteBuilder MapEndpointModules(this IEndpointRouteBuilder app)
    {
        var endpointModules = typeof(EndpointExtensions).Assembly
            .GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && typeof(IEndpointModule).IsAssignableFrom(t))
            .Select(Activator.CreateInstance)
            .Cast<IEndpointModule>();

        foreach (var module in endpointModules)
        {
            module.MapEndpoints(app);
        }

        return app;
    }
}
