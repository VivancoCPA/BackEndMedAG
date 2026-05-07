using SamplVSSkill.Domain.Common;

namespace SamplVSSkill.Domain.Entities;

public class Doctor : IHasTimestamps
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public int? SpecialtyId { get; set; }           // FK → specialties.id
    public string? Register { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? PhotoUrl { get; set; }
    public bool IsVet { get; set; } = false;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
