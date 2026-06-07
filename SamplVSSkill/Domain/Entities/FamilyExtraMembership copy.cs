namespace SamplVSSkill.Domain.Entities;

/// <summary>
/// Representa la vinculación de un usuario a un grupo familiar (many-to-many con atributos adicionales).
/// </summary>
public class FamilyExtraMembership
{
    public int Id { get; set; }

    public string FullName { get; set; } = string.Empty;
    public string IdType { get; set; } = string.Empty;
    public string PhotoUrl { get; set; } = string.Empty;

    public Guid FamilyGroupId { get; set; }
    public FamilyGroup? FamilyGroup { get; set; }

    public string Description { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
