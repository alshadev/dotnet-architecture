namespace Arusha.Template.Domain.IntegrationEventLogs;

// Composite primary key with eventid, type and event handler type
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
            throw new ArgumentException("Cannot mark an event as processed if it is not currently being processed.");

        Status = IntegrationEventLogStatus.Processed;
    }

    public void Failed(string remark)
    {
        if (Status != IntegrationEventLogStatus.Processing)
            throw new ArgumentException("Cannot mark an event as failed if it is not currently being processed.");

        Status = IntegrationEventLogStatus.Failed;
        Remark = remark;
    }

    public void Retry()
    {
        if (Status == IntegrationEventLogStatus.Processing)
            throw new ArgumentException("Cannot retry an event that is currently being processed.");

        LastAttemptOnUtc = DateTime.UtcNow;
        Status = IntegrationEventLogStatus.Processing;
    }
}
