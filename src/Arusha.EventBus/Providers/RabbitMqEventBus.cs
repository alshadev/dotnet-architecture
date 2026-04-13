using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Arusha.EventBus.Providers;

internal sealed class RabbitMqEventBus(
    IConnection connection,
    IServiceProvider serviceProvider,
    ILogger<RabbitMqEventBus> logger) : IEventBus
{
    private const string ExchangeName = "arusha.eventbus";

    private readonly Dictionary<string, (IChannel Channel, Type HandlerType)> _consumers = [];

    private static readonly Lazy<Dictionary<string, Type>> _eventNameCache = new(() => AppDomain
        .CurrentDomain
        .GetAssemblies()
        .SelectMany(a => a.GetTypes())
        .Where(t => typeof(IntegrationEvent).IsAssignableFrom(t) && !t.IsAbstract)
        .ToDictionary(t => t.Name, t => t));

    private static bool TryGetEventType(string eventName, out Type eventType)
        => _eventNameCache.Value.TryGetValue(eventName, out eventType);

    public async Task PublishAsync<T>(T @event) where T : IntegrationEvent
    {
        using var scope = serviceProvider.CreateScope();
        var logService = scope.ServiceProvider.GetRequiredService<IIntegrationEventLogService>();

        await logService.AddOrSetRetryPublisherLogAsync(@event);

        string eventName = @event.GetType().Name;

        try
        {
            var channel = await connection.CreateChannelAsync();
            await channel.ExchangeDeclareAsync(ExchangeName, ExchangeType.Topic, durable: true);

            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(@event));
            var properties = new BasicProperties
            {
                ContentType = "application/json",
                DeliveryMode = DeliveryModes.Persistent,
                Headers = new Dictionary<string, object> { ["Event"] = eventName }
            };

            await channel.BasicPublishAsync(ExchangeName, eventName, mandatory: false, properties, body);
            await channel.CloseAsync();

            await logService.MarkAsProcessedAsync(@event.Id, IntegrationEventLogType.Publisher, string.Empty);
            logger.LogInformation("[RabbitMQ] Published event {EventId} of name {EventName}", @event.Id, eventName);
        }
        catch (Exception ex)
        {
            await logService.MarkAsFailedAsync(@event.Id, IntegrationEventLogType.Publisher, string.Empty, ex.Message);
            logger.LogError(ex, "[RabbitMQ] Failed to publish event {EventId} of name {EventName}", @event.Id, eventName);
        }
    }

    public async Task SubscribeAsync<T, TH>(string serviceName)
        where T : IntegrationEvent
        where TH : IIntegrationEventHandler<T>
    {
        string eventName = typeof(T).Name;
        var eventHandlerType = typeof(TH);
        string eventHandlerName = eventHandlerType.Name;
        string cleanHandlerName = eventHandlerName.EndsWith("EventHandler", StringComparison.InvariantCulture)
            ? eventHandlerName[..^"EventHandler".Length]
            : eventHandlerName;

        string queueName = $"{serviceName.Trim()}.{cleanHandlerName}".ToLowerInvariant();

        logger.LogInformation("[RabbitMQ] Subscribing event '{EventName}' with handler '{EventHandlerName}'", eventName, eventHandlerName);

        if (_consumers.ContainsKey(queueName)) return;

        var channel = await connection.CreateChannelAsync();
        await channel.ExchangeDeclareAsync(ExchangeName, ExchangeType.Topic, durable: true);
        await channel.QueueDeclareAsync(queueName, durable: true, exclusive: false, autoDelete: false);
        await channel.QueueBindAsync(queueName, ExchangeName, eventName);
        await channel.BasicQosAsync(0, 10, false);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (_, ea) =>
        {
            using var scope = serviceProvider.CreateScope();

            string messageEventName = eventName;

            if (ea.BasicProperties.Headers != null
                && ea.BasicProperties.Headers.TryGetValue("Event", out var headerObj)
                && headerObj is byte[] headerBytes)
            {
                messageEventName = Encoding.UTF8.GetString(headerBytes);
            }

            if (!_eventNameCache.Value.TryGetValue(messageEventName, out var eventType))
            {
                logger.LogWarning("[RabbitMQ] Unregistered event type: {Event}", messageEventName);
                await channel.BasicNackAsync(ea.DeliveryTag, false, true);
                return;
            }

            var messageBody = Encoding.UTF8.GetString(ea.Body.ToArray());
            var integrationEvent = JsonSerializer.Deserialize(messageBody, eventType);

            if (integrationEvent is null)
            {
                logger.LogWarning("[RabbitMQ] Failed to deserialize message to {Event}.", messageEventName);
                await channel.BasicNackAsync(ea.DeliveryTag, false, false);
                return;
            }

            try
            {
                var handler = scope.ServiceProvider.GetService(eventHandlerType);
                if (handler == null)
                {
                    logger.LogError("[RabbitMQ] Handler {HandlerType} not found.", eventHandlerType.Name);
                    await channel.BasicNackAsync(ea.DeliveryTag, false, true);
                    return;
                }

                var handleMethod = eventHandlerType.GetMethod("Handle")!;
                await (Task)handleMethod.Invoke(handler, [integrationEvent])!;

                await channel.BasicAckAsync(ea.DeliveryTag, false);
                logger.LogInformation("[RabbitMQ] Successfully processed event {Event} with handler {HandlerType}.", messageEventName, eventHandlerType.Name);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "[RabbitMQ] Error processing event {Event} with handler {HandlerType}.", messageEventName, eventHandlerType.Name);
                await channel.BasicNackAsync(ea.DeliveryTag, false, true);
            }
        };

        await channel.BasicConsumeAsync(queueName, autoAck: false, consumer);
        _consumers[queueName] = (channel, eventHandlerType);

        logger.LogInformation("[RabbitMQ] Subscribed to queue '{QueueName}' for event '{EventName}'", queueName, eventName);
    }

    public async Task RepublishAsync(IntegrationEventLog eventLog)
    {
        if (eventLog.Type != IntegrationEventLogType.Publisher)
        {
            logger.LogWarning("[RabbitMQ] Skipping Republish: EventLog {EventId} is not of type Publisher.", eventLog.EventId);
            return;
        }

        if (!TryGetEventType(eventLog.Event, out var eventType))
        {
            logger.LogWarning("[RabbitMQ] Event {Event} not found in cache.", eventLog.Event);
            return;
        }

        if (JsonSerializer.Deserialize(eventLog.Payload, eventType) is IntegrationEvent @event)
        {
            await PublishAsync(@event);
        }
    }

    public async Task ReprocessSubscriptionAsync(IntegrationEventLog eventLog)
    {
        if (eventLog.Type != IntegrationEventLogType.Subscriber)
        {
            logger.LogWarning("[RabbitMQ] Skipping Reprocess Subscription: EventLog {EventId} is not of type Subscriber.", eventLog.EventId);
            return;
        }

        if (!TryGetEventType(eventLog.Event, out var eventType))
        {
            logger.LogWarning("[RabbitMQ] Event {Event} not found in cache.", eventLog.Event);
            return;
        }

        if (JsonSerializer.Deserialize(eventLog.Payload, eventType) is not IntegrationEvent integrationEvent)
        {
            logger.LogError("[RabbitMQ] Failed to deserialize event {EventId} with type {Event}.", eventLog.EventId, eventLog.Event);
            return;
        }

        using var scope = serviceProvider.CreateScope();
        var handler = ResolveHandler(scope.ServiceProvider, eventLog.EventHandler, eventType);

        if (handler == null)
        {
            logger.LogError("[RabbitMQ] Event Handler {EventHandler} not found for event {EventId}.", eventLog.EventHandler, eventLog.EventId);
            return;
        }

        var concreteType = typeof(IIntegrationEventHandler<>).MakeGenericType(eventType);
        var handleMethod = concreteType.GetMethod("Handle")!;
        await (Task)handleMethod.Invoke(handler, [integrationEvent])!;
    }

    private static object ResolveHandler(IServiceProvider serviceProvider, string handlerTypeName, Type eventType)
    {
        var handlerInterface = typeof(IIntegrationEventHandler<>).MakeGenericType(eventType);
        var handlers = serviceProvider.GetServices(handlerInterface);

        foreach (var handler in handlers)
        {
            if (handler!.GetType().Name.Equals(handlerTypeName, StringComparison.OrdinalIgnoreCase))
            {
                return handler;
            }
        }

        return null;
    }
}
