namespace Arusha.Template.Domain.Primitives;

/// <summary>
/// Base class for all domain entities.
/// An entity is defined by its identity rather than its attributes.
/// </summary>
/// <typeparam name="TId">The type of the entity's identifier.</typeparam>
public abstract class Entity<TId> : IEquatable<Entity<TId>>
    where TId : notnull
{
    /// <summary>
    /// Gets the unique identifier for this entity.
    /// </summary>
    public TId Id { get; protected init; } = default!;

    protected Entity(TId id)
    {
        Id = id;
    }

    /// <summary>
    /// Protected constructor for EF Core.
    /// </summary>
    protected Entity()
    {
    }

    public static bool operator ==(Entity<TId> left, Entity<TId> right)
    {
        return left is not null && right is not null && left.Equals(right);
    }

    public static bool operator !=(Entity<TId> left, Entity<TId> right)
    {
        return !(left == right);
    }

    public bool Equals(Entity<TId> other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        if (other.GetType() != GetType()) return false;

        return EqualityComparer<TId>.Default.Equals(Id, other.Id);
    }

    public override bool Equals(object obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;

        return obj is Entity<TId> entity && Equals(entity);
    }

    public override int GetHashCode()
    {
        return EqualityComparer<TId>.Default.GetHashCode(Id);
    }
}
