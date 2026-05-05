using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SamplVSSkill.Domain.Common;
using SamplVSSkill.Domain.Entities;
using SamplVSSkill.Domain.Enums;

namespace SamplVSSkill.Infrastructure.Persistence;

/// <summary>
/// EF Core DbContext used for Commands (INSERT, UPDATE, DELETE) and Identity management.
/// Queries (SELECT) are handled by Dapper via DapperConnectionFactory.
/// </summary>
public class AppDbContext : IdentityDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Doctor> Doctors => Set<Doctor>();
    public DbSet<MedicalCenter> MedicalCenters => Set<MedicalCenter>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ── Doctor ──────────────────────────────────────────────
        modelBuilder.Entity<Doctor>(entity =>
        {
            entity.ToTable("doctors");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasColumnName("name").IsRequired();
            entity.Property(e => e.Specialty).HasColumnName("specialty");
            entity.Property(e => e.IsVet).HasColumnName("is_vet").HasDefaultValue(false);
        });

        // ── MedicalCenter ───────────────────────────────────────
        modelBuilder.Entity<MedicalCenter>(entity =>
        {
            entity.ToTable("medical_centers");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasColumnName("name").IsRequired();

            // Enum almacenado como string (varchar) en PostgreSQL
            entity.Property(e => e.Type)
                  .HasColumnName("type")
                  .HasConversion<string>();

            entity.Property(e => e.Address).HasColumnName("address");
            entity.Property(e => e.Phone).HasColumnName("phone");
            entity.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            entity.Property(e => e.Latitude).HasColumnName("latitude");
            entity.Property(e => e.Longitude).HasColumnName("longitude");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        });
    }

    // ── Auto-timestamps ─────────────────────────────────────────
    // Sets UpdatedAt = UtcNow on every write for any entity implementing IHasTimestamps.
    // CreatedAt is set once by the Create handler — this method never overwrites it.
    public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<IHasTimestamps>())
        {
            if (entry.State == EntityState.Modified)
                entry.Entity.UpdatedAt = now;
        }

        return await base.SaveChangesAsync(ct);
    }
}
