namespace SamplVSSkill.Domain.Entities;

/// <summary>
/// Many-to-many: un usuario puede tener múltiples aseguradoras.
/// PK compuesta: (UserId, InsurerId).
/// </summary>
public class UserInsurance
{
    public string UserId { get; set; } = string.Empty;
    public AppUser? User { get; set; }

    public Guid InsurerId { get; set; }
    public Insurer? Insurer { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
