namespace SamplVSSkill.Domain.Entities;

public class UserScope
{
    public int Id { get; set; }
    /// <summary>FK → AspNetUsers.Id (owner/creator del grupo)</summary>
    public string? UserIdAdmin { get; set; }
    public AppUser? UserAdmin { get; set; }
    public string? UserId { get; set; }
    public AppUser? User { get; set; }
}
