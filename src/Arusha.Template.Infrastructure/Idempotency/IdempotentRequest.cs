namespace Arusha.Template.Infrastructure.Idempotency;

/// <summary>
/// Entity for tracking processed requests to ensure idempotency.
/// Prevents duplicate command processing.
/// </summary>
public sealed class IdempotentRequest
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = default!;
    public DateTime CreatedOnUtc { get; private set; }

    private IdempotentRequest() { }

    public static IdempotentRequest Create(Guid id, string name)
    {
        return new IdempotentRequest
        {
            Id = id,
            Name = name,
            CreatedOnUtc = DateTime.UtcNow
        };
    }
}
