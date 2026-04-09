namespace Arusha.Template.Application.IntegrationEvents.Events;

public class OrderCreatedIntegrationEvent : IntegrationEvent
{
    public OrderCreatedIntegrationEvent()
    {
        
    }
}

public class OrderCreatedIntegrationEventHandler(IIntegrationEventLogService integrationEventLogService
    , ILogger<OrderCreatedIntegrationEventHandler> logger)
    : BaseIntegrationEventHandler<OrderCreatedIntegrationEvent>(integrationEventLogService, logger, null)
{
    protected override Task ProcessEvent(OrderCreatedIntegrationEvent @event)
    {
        logger.LogInformation("Processing OrderCreatedIntegrationEvent with Id: {EventId}", @event.Id);

        return Task.CompletedTask;
    }
}
