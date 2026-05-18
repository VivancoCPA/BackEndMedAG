namespace SamplVSSkill.Domain.Entities;

public class DoctorAffiliation
{
    public int Id { get; set; }
    public Guid DoctorId { get; set; }
    public Guid CenterId { get; set; }
    public string? OfficeNumber { get; set; }
    public string? WorkSchedule { get; set; }
    public DateTime CreatedAt { get; set; }

    public Doctor Doctor { get; set; } = null!;
    public MedicalCenter MedicalCenter { get; set; } = null!;
}
