namespace Arusha.Template.Infrastructure.Persistence;

/// <summary>
/// Design-time factory for ApplicationDbContext.
/// Used by EF Core tools (migrations, scaffolding) at design time.
///
/// Configure connection string via environment variable:
///   set ARUSHA_TEMPLATE_CONNECTION_STRING=Server=...;Database=...;
/// </summary>
public sealed class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ARUSHA_TEMPLATE_CONNECTION_STRING")
            ?? throw new InvalidOperationException(
                "Design-time connection string not configured. " +
                "Set the ARUSHA_TEMPLATE_CONNECTION_STRING environment variable.");

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlServer(
            connectionString,
            b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName));

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
