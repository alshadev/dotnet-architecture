using Arusha.EventBus;

namespace Arusha.Template.Application.IntegrationEvents;

public class OrderCreatedIntegrationEvent : IntegrationEvent
{
}

internal class OrderCreatedIntegrationEventHandler(
    IIntegrationEventLogService integrationEventLogService,
    ILogger<OrderCreatedIntegrationEventHandler> logger)
    : BaseIntegrationEventHandler<OrderCreatedIntegrationEvent>(integrationEventLogService, logger)
{
    protected override Task ProcessEvent(OrderCreatedIntegrationEvent @event)
    {
        logger.LogInformation("Processing OrderCreatedIntegrationEvent with Id: {EventId}", @event.Id);

        return Task.CompletedTask;
    }
}
