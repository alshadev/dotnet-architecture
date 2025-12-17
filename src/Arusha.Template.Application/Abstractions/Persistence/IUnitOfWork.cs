namespace Arusha.Template.Application.Abstractions.Persistence;

/// <summary>
/// Represents a unit of work that can commit changes to the database.
/// 
/// ARCHITECTURE NOTE: This interface is in the Application layer (not Domain) because:
/// - Clean Architecture: Interfaces should be in the layer that USES them (Application layer uses this in behaviors/handlers)
/// - Dependency Inversion Principle: Application defines the contract, Infrastructure implements it
/// - Domain layer should only contain core business logic, not persistence abstractions
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Saves all changes made in this unit of work to the database.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The number of state entries written to the database.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Begins a database transaction.
    /// </summary>
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Commits the current database transaction.
    /// </summary>
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Rolls back the current database transaction.
    /// </summary>
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
