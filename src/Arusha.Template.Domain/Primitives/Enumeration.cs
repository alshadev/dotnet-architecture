namespace Arusha.Template.Domain.Primitives;

/// <summary>
/// Base class for smart enumerations.
/// Provides more functionality than standard enums while maintaining type safety.
/// Use for domain concepts that have a fixed set of values with associated behavior.
/// </summary>
/// <typeparam name="TEnum">The enumeration type itself.</typeparam>
public abstract class Enumeration<TEnum> : IEquatable<Enumeration<TEnum>>
    where TEnum : Enumeration<TEnum>
{
    private static readonly Lazy<Dictionary<int, TEnum>> EnumerationsByValue =
        new(() => GetEnumerations().ToDictionary(e => e.Value));

    private static readonly Lazy<Dictionary<string, TEnum>> EnumerationsByName =
        new(() => GetEnumerations().ToDictionary(e => e.Name, StringComparer.OrdinalIgnoreCase));

    /// <summary>
    /// Gets the numeric value of the enumeration.
    /// </summary>
    public int Value { get; }

    /// <summary>
    /// Gets the name of the enumeration.
    /// </summary>
    public string Name { get; }

    protected Enumeration(int value, string name)
    {
        Value = value;
        Name = name;
    }

    /// <summary>
    /// Gets all enumeration values.
    /// </summary>
    public static IReadOnlyCollection<TEnum> GetAll() => EnumerationsByValue.Value.Values.ToList();

    /// <summary>
    /// Gets an enumeration by its numeric value.
    /// </summary>
    public static TEnum FromValue(int value)
    {
        return EnumerationsByValue.Value.TryGetValue(value, out var enumeration) ? enumeration : null;
    }

    /// <summary>
    /// Gets an enumeration by its name.
    /// </summary>
    public static TEnum FromName(string name)
    {
        return EnumerationsByName.Value.TryGetValue(name, out var enumeration) ? enumeration : null;
    }

    /// <summary>
    /// Tries to get an enumeration by its value.
    /// </summary>
    public static bool TryFromValue(int value, out TEnum enumeration)
    {
        return EnumerationsByValue.Value.TryGetValue(value, out enumeration);
    }

    /// <summary>
    /// Tries to get an enumeration by its name.
    /// </summary>
    public static bool TryFromName(string name, out TEnum enumeration)
    {
        return EnumerationsByName.Value.TryGetValue(name, out enumeration);
    }

    public bool Equals(Enumeration<TEnum> other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return GetType() == other.GetType() && Value == other.Value;
    }

    public override bool Equals(object obj)
    {
        return obj is Enumeration<TEnum> other && Equals(other);
    }

    public override int GetHashCode() => Value.GetHashCode();

    public override string ToString() => Name;

    public static bool operator ==(Enumeration<TEnum> left, Enumeration<TEnum> right)
    {
        if (left is null && right is null) return true;
        if (left is null || right is null) return false;
        return left.Equals(right);
    }

    public static bool operator !=(Enumeration<TEnum> left, Enumeration<TEnum> right)
    {
        return !(left == right);
    }

    private static IEnumerable<TEnum> GetEnumerations()
    {
        return typeof(TEnum)
            .GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.DeclaredOnly)
            .Where(field => field.FieldType == typeof(TEnum))
            .Select(field => (TEnum)field.GetValue(null)!)
            .Where(e => e is not null);
    }
}
