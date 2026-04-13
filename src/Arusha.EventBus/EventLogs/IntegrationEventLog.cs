namespace Arusha.EventBus;

public class IntegrationEventLog
{
    public Guid EventId { get; set; }
    public IntegrationEventLogType Type { get; set; }
    public string Event { get; set; }
    public string EventHandler { get; set; }
    public string Payload { get; set; }
    public IntegrationEventLogStatus Status { get; set; }
    public string Remark { get; set; }
    public DateTime CreatedOnUtc { get; set; }
    public DateTime LastAttemptOnUtc { get; set; }

    public void Processed()
    {
        if (Status != IntegrationEventLogStatus.Processing)
            throw new InvalidOperationException("Cannot mark as processed if not currently processing.");

        Status = IntegrationEventLogStatus.Processed;
    }

    public void Failed(string remark)
    {
        if (Status != IntegrationEventLogStatus.Processing)
            throw new InvalidOperationException("Cannot mark as failed if not currently processing.");

        Status = IntegrationEventLogStatus.Failed;
        Remark = remark;
    }

    public void Retry()
    {
        if (Status == IntegrationEventLogStatus.Processing)
            throw new InvalidOperationException("Cannot retry an event that is currently processing.");

        LastAttemptOnUtc = DateTime.UtcNow;
        Status = IntegrationEventLogStatus.Processing;
    }
}
