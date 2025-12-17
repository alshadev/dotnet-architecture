namespace Arusha.Template.Domain.Abstractions;

/// <summary>
/// Interface for entities that support soft deletion.
/// </summary>
public interface ISoftDeletable
{
    bool IsDeleted { get; set; }
    DateTime? DeletedOnUtc { get; set; }
    string DeletedBy { get; set; }
}
