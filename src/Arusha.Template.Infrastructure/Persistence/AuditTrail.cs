namespace Arusha.Template.Infrastructure.Persistence;

/// <summary>
/// Simple audit trail entry persisted for create/update/delete operations.
/// </summary>
public sealed class AuditTrail
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string TableName { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string OldValues { get; set; }
    public string NewValues { get; set; }
    public string PerformedBy { get; set; }
    public DateTime PerformedOnUtc { get; set; }
}
