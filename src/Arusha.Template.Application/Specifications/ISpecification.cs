namespace Arusha.Template.Application.Specifications;

/// <summary>
/// Base interface for the Specification pattern.
/// Encapsulates a business rule that can be evaluated against an entity.
/// </summary>
/// <typeparam name="T">The type of entity this specification applies to.</typeparam>
public interface ISpecification<T>
{
    /// <summary>
    /// Gets the criteria expression for this specification.
    /// </summary>
    Expression<Func<T, bool>> Criteria { get; }

    /// <summary>
    /// Gets the include expressions for eager loading.
    /// </summary>
    IReadOnlyList<Expression<Func<T, object>>> Includes { get; }

    /// <summary>
    /// Gets the string-based include expressions for nested eager loading.
    /// </summary>
    IReadOnlyList<string> IncludeStrings { get; }

    /// <summary>
    /// Gets the order by expression.
    /// </summary>
    Expression<Func<T, object>> OrderBy { get; }

    /// <summary>
    /// Gets the order by descending expression.
    /// </summary>
    Expression<Func<T, object>> OrderByDescending { get; }

    /// <summary>
    /// Gets the number of items to take.
    /// </summary>
    int? Take { get; }

    /// <summary>
    /// Gets the number of items to skip.
    /// </summary>
    int? Skip { get; }

    /// <summary>
    /// Indicates whether paging is enabled.
    /// </summary>
    bool IsPagingEnabled { get; }
}
