namespace SamplVSSkill.Domain.Entities;

public class FamilyGroup
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    /// <summary>FK → AspNetUsers.Id (owner/creator del grupo)</summary>
    public string? UserId { get; set; }
    public AppUser? User { get; set; }
    public string? PhotoUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
