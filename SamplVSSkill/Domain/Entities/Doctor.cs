namespace SamplVSSkill.Domain.Entities;

public class Doctor
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Specialty { get; set; }
    public bool IsVet { get; set; } = false;
}
