namespace Arusha.Template.Domain.Orders;

/// <summary>
/// Strongly typed identifier for OrderItem entities.
/// </summary>
public readonly record struct OrderItemId(Guid Value) : IStronglyTypedId<Guid>
{
    public static OrderItemId New() => new(Guid.NewGuid());

    public static OrderItemId From(Guid value) => new(value);

    public override string ToString() => Value.ToString();

    public static implicit operator Guid(OrderItemId orderItemId) => orderItemId.Value;

    public static implicit operator OrderItemId(Guid value) => new(value);
}
