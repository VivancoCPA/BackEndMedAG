namespace SamplVSSkill.Domain.Enums;

/// <summary>
/// Tipos de centros médicos soportados por el sistema.
/// Almacenado como string en PostgreSQL para legibilidad y portabilidad.
/// </summary>
public enum MedicalCenterType
{
    Hospital,
    Clinica,       // Clínica (sin tilde para compatibilidad con EF/DB)
    Veterinaria,
    Laboratorio,
    PostaMedica
}
