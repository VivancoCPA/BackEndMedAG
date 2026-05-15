using SamplVSSkill.Domain.Common;

namespace SamplVSSkill.Domain.Entities;

public class MedicalCenter : IHasTimestamps
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;

    /// <summary>FK → centers_type.id (nullable, según el schema)</summary>
    public int? TypeId { get; set; }

    /// <summary>Navigation property — cargada sólo cuando se hace Include().</summary>
    public CenterType? CenterType { get; set; }

    public string? Address { get; set; }
    public string? Phone { get; set; }
    public bool IsActive { get; set; } = true;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
