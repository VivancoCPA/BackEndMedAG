namespace SamplVSSkill.Domain.Entities;

/// <summary>
/// Master table for medical/veterinary specialties.
/// PK is int (serial) — DB-generated autoincrement.
/// No UpdatedAt since specialties rarely change. CreatedAt set by Create handler.
/// </summary>
public class Specialty
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
}
