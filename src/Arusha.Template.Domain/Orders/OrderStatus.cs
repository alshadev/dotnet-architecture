namespace Arusha.Template.Domain.Orders;

/// <summary>
/// Smart enumeration for order status.
/// Demonstrates the Enumeration pattern with behavior.
/// </summary>
public sealed class OrderStatus : Enumeration<OrderStatus>
{
    public static readonly OrderStatus Pending = new(1, nameof(Pending));
    public static readonly OrderStatus Confirmed = new(2, nameof(Confirmed));
    public static readonly OrderStatus Shipped = new(3, nameof(Shipped));
    public static readonly OrderStatus Delivered = new(4, nameof(Delivered));
    public static readonly OrderStatus Cancelled = new(5, nameof(Cancelled));

    private OrderStatus(int value, string name) : base(value, name) { }

    /// <summary>
    /// Checks if the order can transition to the specified status.
    /// </summary>
    public bool CanTransitionTo(OrderStatus newStatus)
    {
        return (this, newStatus) switch
        {
            (_, _) when this == newStatus => false, // Same status
            (_, _) when this == Cancelled => false, // Cannot transition from cancelled
            (_, _) when this == Delivered => false, // Cannot transition from delivered
            (_, _) when newStatus == Pending => false, // Cannot go back to pending
            (_, _) when this == Pending && newStatus == Confirmed => true,
            (_, _) when this == Pending && newStatus == Cancelled => true,
            (_, _) when this == Confirmed && newStatus == Shipped => true,
            (_, _) when this == Confirmed && newStatus == Cancelled => true,
            (_, _) when this == Shipped && newStatus == Delivered => true,
            _ => false
        };
    }
}
