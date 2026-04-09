namespace Arusha.Template.Domain.IntegrationEventLogs;

public sealed class IntegrationEventLogType : Enumeration<IntegrationEventLogType>
{
    public static readonly IntegrationEventLogType Publisher = new(1, nameof(Publisher));
    public static readonly IntegrationEventLogType Subscriber = new(2, nameof(Subscriber));

    private IntegrationEventLogType(int value, string name) : base(value, name) { }
}
