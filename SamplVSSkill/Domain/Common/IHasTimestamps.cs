namespace SamplVSSkill.Domain.Common;

/// <summary>
/// Marker interface for entities that track creation and update timestamps.
/// AppDbContext.SaveChangesAsync automatically sets UpdatedAt on every write.
/// CreatedAt is set once by the Create handler and never modified.
/// </summary>
public interface IHasTimestamps
{
    DateTime CreatedAt { get; set; }
    DateTime UpdatedAt { get; set; }
}
