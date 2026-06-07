namespace SamplVSSkill.Domain.Entities;

/// <summary>
/// Representa la vinculación de un usuario a un grupo familiar (many-to-many con atributos adicionales).
/// </summary>
public class FamilyMembership
{
    public int Id { get; set; }

    public string UserId { get; set; } = string.Empty;
    public AppUser? User { get; set; }

    public Guid FamilyGroupId { get; set; }
    public FamilyGroup? FamilyGroup { get; set; }

    public bool IsAdmin { get; set; } = false;
    public string? Relationship { get; set; }

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
