using SamplVSSkill.Domain.Common;

namespace SamplVSSkill.Domain.Entities;

/// <summary>
/// Master catalog for medical center types (Hospital, Clínica, Veterinaria, etc.).
/// PK is int — DB-generated autoincrement (identity).
/// Implements IHasTimestamps: UpdatedAt auto-managed by AppDbContext.SaveChangesAsync.
/// </summary>
public class CenterType : IHasTimestamps
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
