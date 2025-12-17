namespace Arusha.Template.Application.Abstractions.Persistence;

/// <summary>
/// Repository interface for the Order aggregate root.
/// 
/// ARCHITECTURE NOTE: This interface is in the Application layer (not Domain) because:
/// - Clean Architecture: Interfaces belong in the layer that USES them (Application handlers use repositories)
/// - Dependency Inversion Principle: Application defines contracts, Infrastructure implements them
/// - Domain layer should only contain core business logic and domain models
/// 
/// DDD NOTE: Each aggregate root has its own specific repository interface
/// with methods tailored to that aggregate's query needs.
/// </summary>
public interface IOrderRepository
{
    /// <summary>
    /// Gets an order by its ID.
    /// </summary>
    Task<Order> GetByIdAsync(OrderId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an order by its ID with all child entities (OrderItems) eagerly loaded.
    /// Use this when you need to access order items to avoid lazy loading issues.
    /// </summary>
    Task<Order> GetByIdWithItemsAsync(OrderId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all orders for a specific customer, ordered by creation date descending.
    /// Returns orders with items eagerly loaded.
    /// </summary>
    Task<IReadOnlyList<Order>> GetByCustomerIdAsync(string customerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new order to the context.
    /// Changes are not persisted until SaveChangesAsync is called on IUnitOfWork.
    /// </summary>
    void Add(Order order);

    /// <summary>
    /// Marks an order as modified in the context.
    /// Changes are not persisted until SaveChangesAsync is called on IUnitOfWork.
    /// </summary>
    void Update(Order order);

    /// <summary>
    /// Removes an order from the context.
    /// If soft delete is configured, this will mark the order as deleted.
    /// Changes are not persisted until SaveChangesAsync is called on IUnitOfWork.
    /// </summary>
    void Remove(Order order);
}
