namespace Arusha.Template.Domain.Abstractions;

/// <summary>
/// Interface for entities that track creation and modification timestamps.
/// </summary>
public interface IAuditableEntity
{
    /// <summary>
    /// UTC timestamp when the entity was created.
    /// </summary>
    DateTime CreatedOnUtc { get; set; }

    /// <summary>
    /// User identifier that created the entity.
    /// </summary>
    string CreatedBy { get; set; }

    /// <summary>
    /// UTC timestamp when the entity was last modified.
    /// Null if never modified after creation.
    /// </summary>
    DateTime? ModifiedOnUtc { get; set; }

    /// <summary>
    /// User identifier that last modified the entity.
    /// </summary>
    string ModifiedBy { get; set; }
}
