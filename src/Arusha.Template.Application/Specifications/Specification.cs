namespace Arusha.Template.Application.Specifications;

/// <summary>
/// Base class for implementing specifications.
/// Encapsulates query logic and can be combined with other specifications.
/// </summary>
/// <typeparam name="T">The type of entity this specification applies to.</typeparam>
public abstract class Specification<T> : ISpecification<T>
{
    private readonly List<Expression<Func<T, object>>> _includes = [];
    private readonly List<string> _includeStrings = [];

    /// <summary>
    /// Creates a specification with the given criteria.
    /// </summary>
    protected Specification(Expression<Func<T, bool>> criteria = null)
    {
        Criteria = criteria ?? (x => true);
    }

    public Expression<Func<T, bool>> Criteria { get; }
    public IReadOnlyList<Expression<Func<T, object>>> Includes => _includes.AsReadOnly();
    public IReadOnlyList<string> IncludeStrings => _includeStrings.AsReadOnly();
    public Expression<Func<T, object>> OrderBy { get; private set; }
    public Expression<Func<T, object>> OrderByDescending { get; private set; }
    public int? Take { get; private set; }
    public int? Skip { get; private set; }
    public bool IsPagingEnabled { get; private set; }

    /// <summary>
    /// Adds an include expression for eager loading.
    /// </summary>
    protected void AddInclude(Expression<Func<T, object>> includeExpression)
    {
        _includes.Add(includeExpression);
    }

    /// <summary>
    /// Adds a string-based include for nested eager loading.
    /// </summary>
    protected void AddInclude(string includeString)
    {
        _includeStrings.Add(includeString);
    }

    /// <summary>
    /// Sets the ordering expression.
    /// </summary>
    protected void ApplyOrderBy(Expression<Func<T, object>> orderByExpression)
    {
        OrderBy = orderByExpression;
    }

    /// <summary>
    /// Sets the descending ordering expression.
    /// </summary>
    protected void ApplyOrderByDescending(Expression<Func<T, object>> orderByDescExpression)
    {
        OrderByDescending = orderByDescExpression;
    }

    /// <summary>
    /// Applies paging to the specification.
    /// </summary>
    protected void ApplyPaging(int skip, int take)
    {
        Skip = skip;
        Take = take;
        IsPagingEnabled = true;
    }
}
