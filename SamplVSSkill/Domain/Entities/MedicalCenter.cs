using SamplVSSkill.Domain.Enums;

namespace SamplVSSkill.Domain.Entities;

public class MedicalCenter
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public MedicalCenterType? Type { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public bool IsActive { get; set; } = true;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}
