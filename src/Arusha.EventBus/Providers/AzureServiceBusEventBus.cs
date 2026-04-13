using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;

namespace Arusha.EventBus.Providers;

internal sealed class AzureServiceBusEventBus(
    ServiceBusAdministrationClient administrationClient,
    ServiceBusClient client,
    IServiceProvider serviceProvider,
    ILogger<AzureServiceBusEventBus> logger) : IEventBus
{
    private readonly Dictionary<string, (ServiceBusProcessor Processor, Type HandlerType)> _processors = [];

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
            string topicName = eventName.ToLowerInvariant();
            var sender = client.CreateSender(topicName);
            var message = new ServiceBusMessage(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(@event)))
            {
                ApplicationProperties = { ["Event"] = eventName }
            };

            await sender.SendMessageAsync(message);
            await logService.MarkAsProcessedAsync(@event.Id, IntegrationEventLogType.Publisher, string.Empty);

            logger.LogInformation("Published event {EventId} of name {EventName} (mark as processed)", @event.Id, eventName);
        }
        catch (Exception ex)
        {
            await logService.MarkAsFailedAsync(@event.Id, IntegrationEventLogType.Publisher, string.Empty, ex.Message);
            logger.LogError(ex, "Failed to publish event {EventId} of name {EventName} (mark as failed)", @event.Id, eventName);
        }
    }

    public async Task RepublishAsync(IntegrationEventLog eventLog)
    {
        if (eventLog.Type != IntegrationEventLogType.Publisher)
        {
            logger.LogWarning("Skipping Republish: EventLog {EventId} is not of type Publisher.", eventLog.EventId);
            return;
        }

        if (!TryGetEventType(eventLog.Event, out var eventType))
        {
            logger.LogWarning("Event {Event} not found in cache.", eventLog.Event);
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
            logger.LogWarning("Skipping Reprocess Subscription: EventLog {EventId} is not of type Subscriber.", eventLog.EventId);
            return;
        }

        if (!TryGetEventType(eventLog.Event, out var eventType))
        {
            logger.LogWarning("Event {Event} not found in cache.", eventLog.Event);
            return;
        }

        if (JsonSerializer.Deserialize(eventLog.Payload, eventType) is not IntegrationEvent integrationEvent)
        {
            logger.LogError("Failed to deserialize event {EventId} with type {Event}.", eventLog.EventId, eventLog.Event);
            return;
        }

        using var scope = serviceProvider.CreateScope();
        var handler = ResolveHandler(scope.ServiceProvider, eventLog.EventHandler, eventType);

        if (handler == null)
        {
            logger.LogError("Event Handler {EventHandler} not found for event {EventId}.", eventLog.EventHandler, eventLog.EventId);
            return;
        }

        var concreteType = typeof(IIntegrationEventHandler<>).MakeGenericType(eventType);
        var handleMethod = concreteType.GetMethod("Handle")!;
        await (Task)handleMethod.Invoke(handler, [integrationEvent])!;
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

        string topicName = eventName.ToLowerInvariant();
        string subscriptionName = $"{serviceName.Trim()}_{cleanHandlerName}".ToLowerInvariant();

        logger.LogInformation("Subscribing event '{EventName}' with handler '{EventHandlerName}'", eventName, eventHandlerName);

        if (_processors.ContainsKey(subscriptionName))
        {
            return;
        }

        if (!await administrationClient.TopicExistsAsync(topicName))
        {
            await administrationClient.CreateTopicAsync(new CreateTopicOptions(topicName));
            logger.LogInformation("[AzureServiceBus] new topic '{TopicName}' created", topicName);
        }

        if (!await administrationClient.SubscriptionExistsAsync(topicName, subscriptionName))
        {
            await administrationClient.CreateSubscriptionAsync(new CreateSubscriptionOptions(topicName, subscriptionName));
            logger.LogInformation("[AzureServiceBus] new subscription '{SubscriptionName}' created", subscriptionName);
        }

        var processor = client.CreateProcessor(topicName, subscriptionName, new ServiceBusProcessorOptions
        {
            MaxConcurrentCalls = 10,
            AutoCompleteMessages = false,
            PrefetchCount = 20,
            MaxAutoLockRenewalDuration = TimeSpan.FromMinutes(15)
        });

        processor.ProcessMessageAsync += CreateMessageHandler(eventHandlerType);
        processor.ProcessErrorAsync += args =>
        {
            logger.LogError(args.Exception, "Azure Service Bus processing error: {Reason}", args.Exception.Message);
            return Task.CompletedTask;
        };

        _processors[subscriptionName] = (processor, eventHandlerType);
        await processor.StartProcessingAsync();
    }

    private Func<ProcessMessageEventArgs, Task> CreateMessageHandler(Type handlerType)
    {
        return async args =>
        {
            using var scope = serviceProvider.CreateScope();

            if (!args.Message.ApplicationProperties.TryGetValue("Event", out var typeObj))
            {
                logger.LogWarning("Missing Event in message.");
                await args.DeadLetterMessageAsync(args.Message, "MissingEvent", "Event not found in ApplicationProperties");
                return;
            }

            string eventName = typeObj.ToString()!;
            var eventType = _eventNameCache.Value.TryGetValue(eventName, out var type) ? type : null;

            if (eventType == null)
            {
                logger.LogWarning("Unregistered event type: {Event}", eventName);
                await args.AbandonMessageAsync(args.Message);
                return;
            }

            string messageBody = Encoding.UTF8.GetString(args.Message.Body);
            object integrationEvent = JsonSerializer.Deserialize(messageBody, eventType)!;

            if (integrationEvent is null)
            {
                logger.LogWarning("Failed to deserialize {MessageBody} to {Event}.", messageBody, eventName);
                await args.DeadLetterMessageAsync(args.Message, "DeserializationError", "Message Body to Event Type is invalid.");
                return;
            }

            try
            {
                object handler = scope.ServiceProvider.GetService(handlerType)!;

                if (handler == null)
                {
                    logger.LogError("Handler {HandlerType} not found.", handlerType.Name);
                    await args.AbandonMessageAsync(args.Message);
                    return;
                }

                var handleMethod = handlerType.GetMethod("Handle")!;
                await (Task)handleMethod.Invoke(handler, [integrationEvent])!;

                await args.CompleteMessageAsync(args.Message);
                logger.LogInformation("Successfully processed event {Event} with handler {HandlerType}.", eventName, handlerType.Name);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing event {Event} with handler {HandlerType}.", eventName, handlerType.Name);
                await args.AbandonMessageAsync(args.Message);
            }
        };
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
