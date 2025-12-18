namespace Arusha.Template.Domain.Orders;

/// <summary>
/// Entity representing an item within an order.
/// Part of the Order aggregate but not the aggregate root.
/// </summary>
public sealed class OrderItem : Entity<OrderItemId>
{
    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; } = default!;
    public int Quantity { get; private set; }
    public Money UnitPrice { get; private set; } = default!;

    /// <summary>
    /// Calculated property for total price.
    /// </summary>
    public Money TotalPrice => UnitPrice.Multiply(Quantity);

    private OrderItem(OrderItemId id, Guid productId, string productName, int quantity, Money unitPrice)
        : base(id)
    {
        ProductId = productId;
        ProductName = productName;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }

    /// <summary>
    /// Private constructor for EF Core.
    /// </summary>
    private OrderItem() : base() { }

    internal static OrderItem Create(Guid productId, string productName, int quantity, Money unitPrice)
    {
        if (productId == Guid.Empty)
            throw new ArgumentException("Product ID is required.", nameof(productId));
        if (string.IsNullOrWhiteSpace(productName))
            throw new ArgumentException("Product name is required.", nameof(productName));
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));

        return new OrderItem(
            OrderItemId.New(),
            productId,
            productName,
            quantity,
            unitPrice);
    }

    /// <summary>
    /// Updates the quantity of the item.
    /// </summary>
    internal void UpdateQuantity(int newQuantity)
    {
        if (newQuantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero.", nameof(newQuantity));

        Quantity = newQuantity;
    }
}
