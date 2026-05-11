namespace SamplVSSkill.Domain.Common;

/// <summary>
/// Lightweight DTO for catalog/dropdown lookups — for entities with int PK.
/// Returns only the minimum required fields: Id + Name.
/// Used by multiple features — defined here to avoid duplication between slices.
/// </summary>
public record LookupItem(int Id, string Name);

/// <summary>
/// Lightweight DTO for catalog/dropdown lookups — for entities with Guid PK (e.g. Insurers).
/// </summary>
public record LookupItemGuid(Guid Id, string Name);
