namespace Arusha.Template.Application.Behaviors;

/// <summary>
/// Pipeline behavior that validates the request using FluentValidation.
/// Order: 2 (after logging)
///
/// Returns a ValidationResult with errors instead of throwing exceptions,
/// maintaining consistency with the Result pattern used throughout the application.
/// </summary>
/// <typeparam name="TRequest">The type of the request.</typeparam>
/// <typeparam name="TResponse">The type of the response.</typeparam>
public sealed class ValidationBehavior<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        if (failures.Count != 0)
        {
            return CreateValidationResult<TResponse>(failures);
        }

        return await next();
    }

    /// <summary>
    /// Creates a ValidationResult that matches the expected TResponse type.
    /// Supports both Result and Result&lt;TValue&gt; responses.
    /// </summary>
    private static TResponse CreateValidationResult<T>(List<ValidationFailure> failures)
    {
        var errors = failures
            .Select(f => Error.Validation(f.PropertyName, f.ErrorMessage))
            .ToArray();

        // If TResponse is Result (non-generic), return ValidationResult
        if (typeof(TResponse) == typeof(Result))
        {
            return (TResponse)(object)Results.ValidationResult.WithErrors(errors);
        }

        // If TResponse is Result<TValue>, return ValidationResult<TValue>
        // Extract the TValue from Result<TValue>
        var resultType = typeof(TResponse);
        if (resultType.IsGenericType && resultType.GetGenericTypeDefinition() == typeof(Result<>))
        {
            var valueType = resultType.GetGenericArguments()[0];
            var validationResultType = typeof(Results.ValidationResult<>).MakeGenericType(valueType);
            var withErrorsMethod = validationResultType.GetMethod(
                nameof(Results.ValidationResult.WithErrors),
                [typeof(Error[])]);

            return (TResponse)withErrorsMethod!.Invoke(null, [errors])!;
        }

        // Fallback: throw if response type is not a Result type
        throw new InvalidOperationException(
            $"Validation failed but response type {typeof(TResponse).Name} is not a Result type.");
    }
}
