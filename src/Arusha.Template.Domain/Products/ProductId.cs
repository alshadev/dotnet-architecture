namespace Arusha.Template.Domain.Products;

/// <summary>
/// Strongly typed identifier for Product entities.
/// </summary>
public readonly record struct ProductId(Guid Value) : IStronglyTypedId<Guid>
{
    public static ProductId New() => new(Guid.NewGuid());

    public static ProductId From(Guid value) => new(value);

    public override string ToString() => Value.ToString();

    public static implicit operator Guid(ProductId productId) => productId.Value;

    public static implicit operator ProductId(Guid value) => new(value);
}
