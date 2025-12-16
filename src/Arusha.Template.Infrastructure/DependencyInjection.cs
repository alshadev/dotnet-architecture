using Arusha.Template.Domain.Repositories;
using Arusha.Template.Infrastructure.Data;
using Arusha.Template.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Arusha.Template.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseInMemoryDatabase("ProductsDb"));

        services.AddScoped<IProductRepository, ProductRepository>();

        return services;
    }
}
