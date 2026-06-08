using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SamplVSSkill.Domain.Common;
using SamplVSSkill.Domain.Entities;

namespace SamplVSSkill.Infrastructure.Persistence;

/// <summary>
/// EF Core DbContext used for Commands (INSERT, UPDATE, DELETE) and Identity management.
/// Queries (SELECT) are handled by Dapper via DapperConnectionFactory.
/// </summary>
public class AppDbContext : IdentityDbContext<AppUser, ApplicationRole, string>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Doctor> Doctors => Set<Doctor>();
    public DbSet<MedicalCenter> MedicalCenters => Set<MedicalCenter>();
    public DbSet<Specialty> Specialties => Set<Specialty>();
    public DbSet<Insurer> Insurers => Set<Insurer>();
    public DbSet<CenterType> CenterTypes => Set<CenterType>();
    public DbSet<DoctorAffiliation> DoctorAffiliations => Set<DoctorAffiliation>();
    public DbSet<FamilyGroup> FamilyGroups => Set<FamilyGroup>();
    public DbSet<UserInsurance> UserInsurances => Set<UserInsurance>();
    public DbSet<FamilyMembership> FamilyMemberships => Set<FamilyMembership>();
    public DbSet<FamilyExtraMembership> FamilyExtraMemberships => Set<FamilyExtraMembership>();
    public DbSet<UserScope> UserScopes => Set<UserScope>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ── AppUser (Identity extended user) ─────────────────────
        // No extra FK config needed — InsurerId removido (ver user_insurances).
        // EF Core detecta automáticamente los campos extra (Name, LastName, etc.)


        // ── UserInsurance (many-to-many: user ↔ insurer) ───────────────
        modelBuilder.Entity<UserInsurance>(entity =>
        {
            entity.ToTable("user_insurances");
            entity.HasKey(e => new { e.UserId, e.InsurerId });
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.InsurerId).HasColumnName("insurer_id");
            entity.Property(e => e.CreatedAt).HasColumnName("CreatedAt");

            entity.HasOne(e => e.User)
                  .WithMany()
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Insurer)
                  .WithMany()
                  .HasForeignKey(e => e.InsurerId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ── FamilyGroup ───────────────────────────────────────
        modelBuilder.Entity<FamilyGroup>(entity =>
        {
            entity.ToTable("family_groups");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasColumnName("name").IsRequired();
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.PhotoUrl).HasColumnName("photo_url");
            entity.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");

            // FK: FamilyGroup → AppUser (owner)
            entity.HasOne(e => e.User)
                  .WithMany()
                  .HasForeignKey(e => e.UserId)
                  .IsRequired(false)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // ── FamilyMembership ───────────────────────────────────
        modelBuilder.Entity<FamilyMembership>(entity =>
        {
            entity.ToTable("family_memberships");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(e => e.UserId).HasColumnName("user_id").IsRequired();
            entity.Property(e => e.FamilyGroupId).HasColumnName("family_group_id").IsRequired();
            entity.Property(e => e.IsAdmin).HasColumnName("is_admin").HasDefaultValue(false);
            entity.Property(e => e.Relationship).HasColumnName("relationship");
            entity.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            // FK: FamilyMembership → AppUser
            entity.HasOne(e => e.User)
                  .WithMany()
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            // FK: FamilyMembership → FamilyGroup
            entity.HasOne(e => e.FamilyGroup)
                  .WithMany()
                  .HasForeignKey(e => e.FamilyGroupId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Unique index to prevent duplicate user. One user can only have one membership.
            entity.HasIndex(e => new { e.UserId }).IsUnique();
        });

        // ── FamilyMembership ───────────────────────────────────
        modelBuilder.Entity<FamilyExtraMembership>(entity =>
        {
            entity.ToTable("family_extra_memberships");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(e => e.FullName).HasColumnName("full_name").IsRequired();
            entity.Property(e => e.IdType).HasColumnName("id_type").IsRequired();
            entity.Property(e => e.PhotoUrl).HasColumnName("photo_url");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.FamilyGroupId).HasColumnName("family_group_id").IsRequired();
            entity.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
           

            // FK: FamilyMembership → FamilyGroup
            entity.HasOne(e => e.FamilyGroup)
                  .WithMany()
                  .HasForeignKey(e => e.FamilyGroupId)
                  .OnDelete(DeleteBehavior.Cascade);

        });

        // ── CenterType ───────────────────────────────────────
        modelBuilder.Entity<CenterType>(entity =>
        {
            entity.ToTable("centers_type");
            entity.HasKey(e => e.Id);
            // int identity — DB generates the value on insert
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(e => e.Name).HasColumnName("name").IsRequired();
            entity.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        });

        // ── Specialty ───────────────────────────────────────────
        modelBuilder.Entity<Specialty>(entity =>
        {
            entity.ToTable("specialties");
            entity.HasKey(e => e.Id);
            // int serial — DB generates the value on insert
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(e => e.Name).HasColumnName("name").IsRequired();
            entity.Property(e => e.Description).HasColumnName("description").IsRequired();
            entity.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
        });

        // ── Doctor ──────────────────────────────────────────────
        modelBuilder.Entity<Doctor>(entity =>
        {
            entity.ToTable("doctors");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasColumnName("name").IsRequired();
            entity.Property(e => e.LastName).HasColumnName("last_name").IsRequired();
            entity.Property(e => e.SpecialtyId).HasColumnName("specialty_id");
            entity.Property(e => e.Register).HasColumnName("register");
            entity.Property(e => e.Phone).HasColumnName("phone");
            entity.Property(e => e.Email).HasColumnName("email");
            entity.Property(e => e.PhotoUrl).HasColumnName("photo_url");
            entity.Property(e => e.IsVet).HasColumnName("is_vet").HasDefaultValue(false);
            entity.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            // FK: Doctor → Specialty
            entity.HasOne<Specialty>()
                  .WithMany()
                  .HasForeignKey(e => e.SpecialtyId)
                  .IsRequired(false)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // ── MedicalCenter ───────────────────────────────────────
        modelBuilder.Entity<MedicalCenter>(entity =>
        {
            entity.ToTable("medical_centers");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasColumnName("name").IsRequired();

            // FK → centers_type.id (type_id column)
            entity.Property(e => e.TypeId).HasColumnName("type_id");

            entity.Property(e => e.Address).HasColumnName("address");
            entity.Property(e => e.Phone).HasColumnName("phone");
            entity.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            entity.Property(e => e.Latitude).HasColumnName("latitude");
            entity.Property(e => e.Longitude).HasColumnName("longitude");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            // Relación: MedicalCenter → CenterType (nullable)
            entity.HasOne(e => e.CenterType)
                  .WithMany()
                  .HasForeignKey(e => e.TypeId)
                  .IsRequired(false)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // ── Insurer ─────────────────────────────────────────────
        modelBuilder.Entity<Insurer>(entity =>
        {
            entity.ToTable("insurers");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasColumnName("name").IsRequired();
            entity.Property(e => e.Address).HasColumnName("address").IsRequired();
            entity.Property(e => e.Phone).HasColumnName("phone").IsRequired();
            entity.Property(e => e.Email).HasColumnName("email").IsRequired();
            entity.Property(e => e.PersonInCharge).HasColumnName("person_in_charge");
            entity.Property(e => e.LogoUrl).HasColumnName("logo_url");
            entity.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        });

        // ── DoctorAffiliation ───────────────────────────────────
        modelBuilder.Entity<DoctorAffiliation>(entity =>
        {
            entity.ToTable("doctor_affiliations");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(e => e.DoctorId).HasColumnName("doctor_id").IsRequired();
            entity.Property(e => e.CenterId).HasColumnName("center_id").IsRequired();
            entity.Property(e => e.OfficeNumber).HasColumnName("office_number");
            entity.Property(e => e.WorkSchedule).HasColumnName("work_schedule");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");

            // Unique index
            entity.HasIndex(e => new { e.DoctorId, e.CenterId }).IsUnique();

            entity.HasOne(e => e.Doctor)
                  .WithMany()
                  .HasForeignKey(e => e.DoctorId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.MedicalCenter)
                  .WithMany()
                  .HasForeignKey(e => e.CenterId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ── UserScope ───────────────────────────────────────
        modelBuilder.Entity<UserScope>(entity =>
        {
            entity.ToTable("user_scope");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.UserIdAdmin).HasColumnName("user_id_admin").IsRequired();
            entity.Property(e => e.UserId).HasColumnName("user_id").IsRequired();

            // FK: UserScope → AspNetUsers.Id (admin)
            entity.HasOne(e => e.UserAdmin)//
                  .WithMany()
                  .HasForeignKey(e => e.UserIdAdmin)
                  .IsRequired(false)
                  .OnDelete(DeleteBehavior.SetNull);
            // FK: UserScope → AspNetUsers.Id (user)
            entity.HasOne(e => e.User)  
                  .WithMany()
                  .HasForeignKey(e => e.UserId)
                  .IsRequired(false)
                  .OnDelete(DeleteBehavior.SetNull);
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
