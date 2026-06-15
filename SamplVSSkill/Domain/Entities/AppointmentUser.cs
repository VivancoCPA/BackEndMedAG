namespace SamplVSSkill.Domain.Entities;

public class AppointmentUser
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public Guid? CenterId { get; set; }
    public Guid? DoctorId { get; set; }
    public int? SpecialtieId { get; set; }
    public Guid? InsurerId { get; set; }
    
    public string Description { get; set; } = string.Empty;
    public DateTime? AppointmentDate { get; set; }
    public DateTime CreatedAt { get; set; }= DateTime.UtcNow;
    public string StatusId { get; set; } = string.Empty;

    public AppUser User { get; set; } = null!;
    public MedicalCenter? MedicalCenter { get; set; } = null!;
    public Doctor? Doctor { get; set; } = null!;
    public Specialty? Specialty { get; set; } = null!;
    public Insurer? Insurer { get; set; } = null!;
    
}    

