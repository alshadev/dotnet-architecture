namespace Arusha.Template.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for Product entity.
/// Implements IProductRepository from Application layer (Dependency Inversion Principle).
/// </summary>
internal sealed class ProductRepository(ApplicationDbContext context) : IProductRepository
{
    public async Task<Product> GetByIdAsync(ProductId id, CancellationToken cancellationToken = default)
    {
        return await context.Products
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<Product> GetBySkuAsync(string sku, CancellationToken cancellationToken = default)
    {
        return await context.Products
            .FirstOrDefaultAsync(p => p.Sku == sku, cancellationToken);
    }

    public async Task<IReadOnlyList<Product>> GetAllActiveAsync(CancellationToken cancellationToken = default)
    {
        return await context.Products
            .Where(p => p.IsActive)
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Product>> GetByCategoryAsync(
        string category,
        CancellationToken cancellationToken = default)
    {
        return await context.Products
            .Where(p => p.Category == category && p.IsActive)
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsBySkuAsync(string sku, CancellationToken cancellationToken = default)
    {
        return await context.Products
            .AnyAsync(p => p.Sku == sku, cancellationToken);
    }

    public void Add(Product product)
    {
        context.Products.Add(product);
    }

    public void Update(Product product)
    {
        context.Products.Update(product);
    }

    public void Remove(Product product)
    {
        context.Products.Remove(product);
    }
}
