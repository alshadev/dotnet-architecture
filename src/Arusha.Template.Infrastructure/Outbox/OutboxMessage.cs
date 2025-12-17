namespace Arusha.Template.Infrastructure.Outbox;

/// <summary>
/// Entity representing an outbox message for reliable event publishing.
/// Implements the Transactional Outbox pattern.
/// </summary>
public sealed class OutboxMessage
{
    public Guid Id { get; private set; }
    public string Type { get; private set; } = default!;
    public string Content { get; private set; } = default!;
    public DateTime OccurredOnUtc { get; private set; }
    public DateTime ProcessedOnUtc { get; private set; }
    public string Error { get; private set; }
    public int RetryCount { get; private set; }

    private OutboxMessage() { }

    public static OutboxMessage Create(IDomainEvent domainEvent)
    {
        var type = domainEvent.GetType().AssemblyQualifiedName!;
        var content = JsonSerializer.Serialize(domainEvent, domainEvent.GetType());

        return new OutboxMessage
        {
            Id = domainEvent.Id,
            Type = type,
            Content = content,
            OccurredOnUtc = domainEvent.OccurredOnUtc
        };
    }

    public void MarkAsProcessed()
    {
        ProcessedOnUtc = DateTime.UtcNow;
    }

    public void MarkAsFailed(string error)
    {
        Error = error;
        RetryCount++;
    }
}
