namespace Arusha.Template.Domain.Orders;

/// <summary>
/// Strongly typed identifier for Order entities.
/// Provides type safety and prevents mixing different ID types.
/// </summary>
public readonly record struct OrderId(Guid Value) : IStronglyTypedId<Guid>
{
    public static OrderId New() => new(Guid.NewGuid());

    public static OrderId From(Guid value) => new(value);

    public override string ToString() => Value.ToString();

    public static implicit operator Guid(OrderId orderId) => orderId.Value;

    public static implicit operator OrderId(Guid value) => new(value);
}
