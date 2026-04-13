namespace Arusha.EventBus;

public abstract class IntegrationEvent
{
    public Guid Id { get; set; }
    public DateTime CreatedOnUtc { get; set; }

    protected IntegrationEvent()
    {
        Id = Guid.NewGuid();
        CreatedOnUtc = DateTime.UtcNow;
    }
}
