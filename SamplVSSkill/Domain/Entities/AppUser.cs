using Microsoft.AspNetCore.Identity;

namespace SamplVSSkill.Domain.Entities;

/// <summary>
/// Extended Identity user with domain-specific profile fields.
/// Maps to the AspNetUsers table (EF Core adds extra columns automatically via migration).
/// </summary>
public class AppUser : IdentityUser
{
    public string Name { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime? DateOfBirth { get; set; }
    public string? PhotoUrl { get; set; }
    public string? Address { get; set; }
    public bool PasswordConfirmed { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastAccess { get; set; }
    // InsurerId eliminado: las aseguradoras se gestionan en user_insurances (many-to-many)

    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }
}
