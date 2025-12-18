namespace Arusha.Template.Application.Results;

/// <summary>
/// Extension methods for Result types.
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Maps a successful result to a new value.
    /// </summary>
    public static Result<TOut> Map<TIn, TOut>(
        this Result<TIn> result,
        Func<TIn, TOut> mapper)
    {
        return result.IsSuccess
            ? Result.Success(mapper(result.Value))
            : Result.Failure<TOut>(result.Error);
    }

    /// <summary>
    /// Binds a successful result to another result-returning operation.
    /// </summary>
    public static Result<TOut> Bind<TIn, TOut>(
        this Result<TIn> result,
        Func<TIn, Result<TOut>> binder)
    {
        return result.IsSuccess
            ? binder(result.Value)
            : Result.Failure<TOut>(result.Error);
    }

    /// <summary>
    /// Executes an action if the result is successful.
    /// </summary>
    public static Result<T> Tap<T>(
        this Result<T> result,
        Action<T> action)
    {
        if (result.IsSuccess)
        {
            action(result.Value);
        }

        return result;
    }

    /// <summary>
    /// Matches on the result and executes the appropriate function.
    /// </summary>
    public static TOut Match<TIn, TOut>(
        this Result<TIn> result,
        Func<TIn, TOut> onSuccess,
        Func<Error, TOut> onFailure)
    {
        return result.IsSuccess
            ? onSuccess(result.Value)
            : onFailure(result.Error);
    }

    /// <summary>
    /// Matches on the result and executes the appropriate function.
    /// </summary>
    public static TOut Match<TOut>(
        this Result result,
        Func<TOut> onSuccess,
        Func<Error, TOut> onFailure)
    {
        return result.IsSuccess
            ? onSuccess()
            : onFailure(result.Error);
    }

    /// <summary>
    /// Converts a result to its value or a default value on failure.
    /// </summary>
    public static T GetValueOrDefault<T>(this Result<T> result, T defaultValue = default)
    {
        return result.IsSuccess ? result.Value : defaultValue;
    }

    /// <summary>
    /// Ensures a condition is met on the result's value.
    /// </summary>
    public static Result<T> Ensure<T>(
        this Result<T> result,
        Func<T, bool> predicate,
        Error error)
    {
        if (result.IsFailure)
            return result;

        return predicate(result.Value)
            ? result
            : Result.Failure<T>(error);
    }

    /// <summary>
    /// Combines multiple results into a single result.
    /// Returns the first failure encountered, or success if all succeed.
    /// </summary>
    public static Result Combine(params Result[] results)
    {
        foreach (var result in results)
        {
            if (result.IsFailure)
                return result;
        }

        return Result.Success();
    }

    /// <summary>
    /// Combines multiple results and returns all errors if any fail.
    /// </summary>
    public static Result CombineAll(params Result[] results)
    {
        var errors = results
            .Where(r => r.IsFailure)
            .Select(r => r.Error)
            .ToArray();

        return errors.Length != 0
            ? ValidationResult.WithErrors(errors)
            : Result.Success();
    }

    /// <summary>
    /// Asynchronously maps a successful result to a new value.
    /// </summary>
    public static async Task<Result<TOut>> MapAsync<TIn, TOut>(
        this Task<Result<TIn>> resultTask,
        Func<TIn, TOut> mapper)
    {
        var result = await resultTask;
        return result.Map(mapper);
    }

    /// <summary>
    /// Asynchronously binds a successful result to another result-returning operation.
    /// </summary>
    public static async Task<Result<TOut>> BindAsync<TIn, TOut>(
        this Task<Result<TIn>> resultTask,
        Func<TIn, Task<Result<TOut>>> binder)
    {
        var result = await resultTask;

        if (result.IsFailure)
            return Result.Failure<TOut>(result.Error);

        return await binder(result.Value);
    }

    /// <summary>
    /// Asynchronously matches on the result.
    /// </summary>
    public static async Task<TOut> MatchAsync<TIn, TOut>(
        this Task<Result<TIn>> resultTask,
        Func<TIn, TOut> onSuccess,
        Func<Error, TOut> onFailure)
    {
        var result = await resultTask;
        return result.Match(onSuccess, onFailure);
    }
}
