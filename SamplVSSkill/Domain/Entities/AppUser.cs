using Microsoft.AspNetCore.Identity;
using SamplVSSkill.Domain.Entities;

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

    /// <summary>FK → insurers.id (nullable)</summary>
    public Guid? InsurerId { get; set; }

    /// <summary>Navigation property — loaded only when included explicitly.</summary>
    public Insurer? Insurer { get; set; }
}
