namespace Arusha.Template.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for Order aggregate.
/// Implements IOrderRepository from Application layer (Dependency Inversion Principle).
/// </summary>
internal sealed class OrderRepository(ApplicationDbContext context) : IOrderRepository
{
    public async Task<Order> GetByIdAsync(OrderId id, CancellationToken cancellationToken = default)
    {
        return await context.Orders
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }

    public async Task<Order> GetByIdWithItemsAsync(OrderId id, CancellationToken cancellationToken = default)
    {
        return await context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Order>> GetByCustomerIdAsync(
        string customerId,
        CancellationToken cancellationToken = default)
    {
        return await context.Orders
            .Include(o => o.Items)
            .Where(o => o.CustomerId == customerId)
            .OrderByDescending(o => o.CreatedOnUtc)
            .ToListAsync(cancellationToken);
    }

    public void Add(Order order)
    {
        context.Orders.Add(order);
    }

    public void Update(Order order)
    {
        context.Orders.Update(order);
    }

    public void Remove(Order order)
    {
        context.Orders.Remove(order);
    }
}
