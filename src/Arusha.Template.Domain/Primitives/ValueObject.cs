namespace Arusha.Template.Domain.Primitives;

/// <summary>
/// Base class for value objects.
/// A value object is defined by its attributes rather than a unique identity.
/// Value objects are immutable and compared by their property values.
/// </summary>
public abstract class ValueObject : IEquatable<ValueObject>
{
    /// <summary>
    /// Gets the atomic values that define equality for this value object.
    /// </summary>
    protected abstract IEnumerable<object> GetEqualityComponents();

    public static bool operator ==(ValueObject left, ValueObject right)
    {
        if (left is null && right is null) return true;
        if (left is null || right is null) return false;
        return left.Equals(right);
    }

    public static bool operator !=(ValueObject left, ValueObject right)
    {
        return !(left == right);
    }

    public bool Equals(ValueObject other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        if (other.GetType() != GetType()) return false;

        return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    }

    public override bool Equals(object obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;

        return obj is ValueObject valueObject && Equals(valueObject);
    }

    public override int GetHashCode()
    {
        return GetEqualityComponents()
            .Aggregate(default(HashCode), (hash, obj) =>
            {
                hash.Add(obj);
                return hash;
            })
            .ToHashCode();
    }
}
