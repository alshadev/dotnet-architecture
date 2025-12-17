namespace Arusha.Template.Application.Abstractions.Persistence;

/// <summary>
/// Repository interface for Product entity.
/// 
/// ARCHITECTURE NOTE: This interface is in the Application layer (not Domain) because:
/// - Clean Architecture: Interfaces belong in the layer that USES them (Application handlers use repositories)
/// - Dependency Inversion Principle: Application defines contracts, Infrastructure implements them
/// - Domain layer should only contain core business logic and domain models
/// 
/// PRAGMATIC DDD NOTE: Product is a simple entity (not a rich aggregate), so this repository
/// provides straightforward CRUD operations without complex domain behavior.
/// </summary>
public interface IProductRepository
{
    /// <summary>
    /// Gets a product by its ID.
    /// </summary>
    Task<Product> GetByIdAsync(ProductId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a product by its unique SKU (Stock Keeping Unit).
    /// </summary>
    Task<Product> GetBySkuAsync(string sku, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all active products, ordered by name.
    /// Filters out inactive products.
    /// </summary>
    Task<IReadOnlyList<Product>> GetAllActiveAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all active products in a specific category, ordered by name.
    /// </summary>
    Task<IReadOnlyList<Product>> GetByCategoryAsync(string category, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a product with the given SKU already exists.
    /// Useful for validation before creating new products.
    /// </summary>
    Task<bool> ExistsBySkuAsync(string sku, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new product to the context.
    /// Changes are not persisted until SaveChangesAsync is called on IUnitOfWork.
    /// </summary>
    void Add(Product product);

    /// <summary>
    /// Marks a product as modified in the context.
    /// Changes are not persisted until SaveChangesAsync is called on IUnitOfWork.
    /// </summary>
    void Update(Product product);

    /// <summary>
    /// Removes a product from the context.
    /// If soft delete is configured, this will mark the product as deleted.
    /// Changes are not persisted until SaveChangesAsync is called on IUnitOfWork.
    /// </summary>
    void Remove(Product product);
}
