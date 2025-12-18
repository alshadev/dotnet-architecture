namespace Arusha.Template.Application.Specifications;

/// <summary>
/// Extension methods for combining specifications.
/// </summary>
public static class SpecificationExtensions
{
    /// <summary>
    /// Combines two specifications with AND logic.
    /// </summary>
    public static AndSpecification<T> And<T>(
        this ISpecification<T> left,
        ISpecification<T> right)
    {
        return new AndSpecification<T>(left, right);
    }

    /// <summary>
    /// Combines two specifications with OR logic.
    /// </summary>
    public static OrSpecification<T> Or<T>(
        this ISpecification<T> left,
        ISpecification<T> right)
    {
        return new OrSpecification<T>(left, right);
    }

    /// <summary>
    /// Negates a specification.
    /// </summary>
    public static NotSpecification<T> Not<T>(this ISpecification<T> specification)
    {
        return new NotSpecification<T>(specification);
    }
}

/// <summary>
/// Specification that combines two specifications with AND logic.
/// </summary>
public sealed class AndSpecification<T> : Specification<T>
{
    public AndSpecification(ISpecification<T> left, ISpecification<T> right)
        : base(CombineExpressions(left.Criteria, right.Criteria, Expression.AndAlso))
    {
    }

    private static Expression<Func<T, bool>> CombineExpressions(
        Expression<Func<T, bool>> left,
        Expression<Func<T, bool>> right,
        Func<Expression, Expression, BinaryExpression> combiner)
    {
        var parameter = Expression.Parameter(typeof(T), "x");

        var leftBody = ReplaceParameter(left.Body, left.Parameters[0], parameter);
        var rightBody = ReplaceParameter(right.Body, right.Parameters[0], parameter);

        var combined = combiner(leftBody, rightBody);

        return Expression.Lambda<Func<T, bool>>(combined, parameter);
    }

    private static Expression ReplaceParameter(
        Expression expression,
        ParameterExpression oldParameter,
        ParameterExpression newParameter)
    {
        return new ParameterReplacer(oldParameter, newParameter).Visit(expression);
    }
}

/// <summary>
/// Specification that combines two specifications with OR logic.
/// </summary>
public sealed class OrSpecification<T> : Specification<T>
{
    public OrSpecification(ISpecification<T> left, ISpecification<T> right)
        : base(CombineExpressions(left.Criteria, right.Criteria, Expression.OrElse))
    {
    }

    private static Expression<Func<T, bool>> CombineExpressions(
        Expression<Func<T, bool>> left,
        Expression<Func<T, bool>> right,
        Func<Expression, Expression, BinaryExpression> combiner)
    {
        var parameter = Expression.Parameter(typeof(T), "x");

        var leftBody = ReplaceParameter(left.Body, left.Parameters[0], parameter);
        var rightBody = ReplaceParameter(right.Body, right.Parameters[0], parameter);

        var combined = combiner(leftBody, rightBody);

        return Expression.Lambda<Func<T, bool>>(combined, parameter);
    }

    private static Expression ReplaceParameter(
        Expression expression,
        ParameterExpression oldParameter,
        ParameterExpression newParameter)
    {
        return new ParameterReplacer(oldParameter, newParameter).Visit(expression);
    }
}

/// <summary>
/// Specification that negates another specification.
/// </summary>
public sealed class NotSpecification<T> : Specification<T>
{
    public NotSpecification(ISpecification<T> specification)
        : base(NegateExpression(specification.Criteria))
    {
    }

    private static Expression<Func<T, bool>> NegateExpression(Expression<Func<T, bool>> expression)
    {
        var negated = Expression.Not(expression.Body);
        return Expression.Lambda<Func<T, bool>>(negated, expression.Parameters);
    }
}

/// <summary>
/// Expression visitor that replaces parameter references.
/// </summary>
internal sealed class ParameterReplacer(
    ParameterExpression oldParameter,
    ParameterExpression newParameter) : ExpressionVisitor
{
    protected override Expression VisitParameter(ParameterExpression node)
    {
        return node == oldParameter ? newParameter : base.VisitParameter(node);
    }
}
