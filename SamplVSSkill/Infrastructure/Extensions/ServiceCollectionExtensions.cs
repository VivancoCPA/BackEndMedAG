using System.Text;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SamplVSSkill.Domain.Entities;
using SamplVSSkill.Infrastructure.Auth;
using SamplVSSkill.Infrastructure.Persistence;
using SamplVSSkill.Infrastructure.Services;

namespace SamplVSSkill.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        // ── EF Core (Commands: INSERT, UPDATE, DELETE + Identity) ──
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString));

        // ── Identity ──
        services.AddIdentityCore<AppUser>(options =>
            {
                options.User.RequireUniqueEmail      = true;

                // ── Password Rules ──
                options.Password.RequireNonAlphanumeric = true;  // Ej: *, @, !
                options.Password.RequireUppercase        = true;  // Al menos una mayúscula
                options.Password.RequireLowercase        = true;  // Al menos una minúscula
                options.Password.RequireDigit            = true;  // Al menos un dígito (0-9)
                options.Password.RequiredLength          = 8;     // Mínimo 8 caracteres
            })
            .AddRoles<ApplicationRole>()                          // enables RoleManager<ApplicationRole>
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();                       // enables password reset tokens

        // ── JWT Authentication ──
        var jwtSection = configuration.GetSection("Jwt");
        var key = Encoding.UTF8.GetBytes(
            jwtSection["Key"] ?? throw new InvalidOperationException("Jwt:Key not configured."));

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSection["Issuer"],
                    ValidAudience = jwtSection["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ClockSkew = TimeSpan.Zero
                };
            });

        services.AddAuthorization();

        // ── JWT Token Service ──
        services.AddSingleton<JwtTokenService>();

        // ── Email Service (development logger; swap for production SMTP) ──
        services.AddScoped<IEmailService, LoggerEmailService>();

        // ── Dapper (Queries: SELECT) ──
        services.AddSingleton(new DapperConnectionFactory(connectionString));

        // ── FluentValidation ──
        services.AddValidatorsFromAssembly(typeof(ServiceCollectionExtensions).Assembly);

        return services;
    }
}

