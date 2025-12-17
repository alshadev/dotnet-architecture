namespace Arusha.Template.Application.Behaviors;

/// <summary>
/// Pipeline behavior that logs the request and response.
/// Order: 1 (first in pipeline)
/// </summary>
/// <typeparam name="TRequest">The type of the request.</typeparam>
/// <typeparam name="TResponse">The type of the response.</typeparam>
public sealed class LoggingBehavior<TRequest, TResponse>(
    ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        logger.LogInformation(
            "Processing request {RequestName}: {@Request}",
            requestName,
            request);

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            var response = await next();

            stopwatch.Stop();

            logger.LogInformation(
                "Completed request {RequestName} in {ElapsedMilliseconds}ms",
                requestName,
                stopwatch.ElapsedMilliseconds);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            logger.LogError(
                ex,
                "Request {RequestName} failed after {ElapsedMilliseconds}ms: {ErrorMessage}",
                requestName,
                stopwatch.ElapsedMilliseconds,
                ex.Message);

            throw;
        }
    }
}
