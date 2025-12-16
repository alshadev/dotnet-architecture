using Arusha.Template.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Arusha.Template.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IProductService, ProductService>();

        return services;
    }
}
