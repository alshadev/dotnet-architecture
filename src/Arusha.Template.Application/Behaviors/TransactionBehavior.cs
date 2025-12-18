namespace Arusha.Template.Application.Behaviors;

/// <summary>
/// Pipeline behavior that wraps command handlers in a database transaction.
/// Order: 4 (after idempotency check, before handler)
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
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // Only apply to commands (write operations)
        if (!IsCommand())
        {
            return await next();
        }

        var requestName = typeof(TRequest).Name;

        logger.LogDebug(
            "Beginning transaction for {RequestName}",
            requestName);

        try
        {
            await unitOfWork.BeginTransactionAsync(cancellationToken);

            var response = await next();

            await unitOfWork.CommitTransactionAsync(cancellationToken);

            logger.LogDebug(
                "Transaction committed for {RequestName}",
                requestName);

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

    private static bool IsCommand()
    {
        return typeof(TRequest).GetInterfaces().Any(i =>
            i.IsGenericType &&
            (i.GetGenericTypeDefinition() == typeof(Abstractions.Messaging.ICommand<>) ||
             i == typeof(Abstractions.Messaging.ICommand)));
    }
}
