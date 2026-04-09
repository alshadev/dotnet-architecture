namespace Arusha.Template.Domain.IntegrationEventLogs;

public sealed class IntegrationEventLogStatus : Enumeration<IntegrationEventLogStatus>
{
    public static readonly IntegrationEventLogStatus Processing = new(1, nameof(Processing));
    public static readonly IntegrationEventLogStatus Processed = new(2, nameof(Processed));
    public static readonly IntegrationEventLogStatus Failed = new(2, nameof(Failed));

    private IntegrationEventLogStatus(int value, string name) : base(value, name) { }
}
