namespace Arusha.Template.Application.Behaviors;

/// <summary>
/// Pipeline behavior that wraps command handlers in a database transaction.
/// Order: 3 (after validation, before handler)
///
/// This ensures all changes made by a command handler are committed atomically.
/// Handlers should NOT call SaveChanges - this behavior handles it.
/// </summary>
/// <typeparam name="TRequest">The type of the request.</typeparam>
/// <typeparam name="TResponse">The type of the response.</typeparam>
public sealed class TransactionBehavior<TRequest, TResponse>(
    IUnitOfWork unitOfWork,
    ILogger<TransactionBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    // Cached per closed generic type — evaluated once per TRequest type, not per request
    private static readonly bool _isCommand = typeof(TRequest).GetInterfaces().Any(i =>
        i.IsGenericType
            ? i.GetGenericTypeDefinition() == typeof(ICommand<>)
            : i == typeof(ICommand));

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // Only apply to commands (write operations)
        if (!_isCommand)
        {
            return await next();
        }

        string requestName = typeof(TRequest).Name;

        logger.LogDebug("Beginning transaction for {RequestName}", requestName);

        try
        {
            await unitOfWork.BeginTransactionAsync(cancellationToken);

            var response = await next();

            await unitOfWork.CommitTransactionAsync(cancellationToken);

            logger.LogDebug("Transaction committed for {RequestName}", requestName);

            return response;
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);

            logger.LogError(
                ex,
                "Transaction failed for {RequestName}: {ErrorMessage}",
                requestName,
                ex.Message);

            throw;
        }
    }
}
