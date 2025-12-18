namespace Arusha.Template.Infrastructure.Persistence;

/// <summary>
/// Design-time factory for ApplicationDbContext.
/// Used by EF Core tools (migrations, scaffolding) at design time.
/// </summary>
public sealed class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        
        // Use SQL Server as default for migrations
        optionsBuilder.UseSqlServer(
            "Server=.\\MSSQLSERVERDE;Database=arushatemplate;User Id=sa;Password=@Superadmin123;TrustServerCertificate=True;",
            b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName));

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
