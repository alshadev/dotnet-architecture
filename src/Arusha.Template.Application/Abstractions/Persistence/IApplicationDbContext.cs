namespace Arusha.Template.Application.Abstractions.Persistence;

/// <summary>
/// Abstraction over the EF Core DbContext used by the application layer.
/// Keeps application logic decoupled from infrastructure.
/// </summary>
public interface IApplicationDbContext
{
    DbSet<TEntity> Set<TEntity>() where TEntity : class;

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
