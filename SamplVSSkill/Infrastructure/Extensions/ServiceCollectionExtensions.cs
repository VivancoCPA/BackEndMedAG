using System.Text;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SamplVSSkill.Infrastructure.Auth;
using SamplVSSkill.Infrastructure.Persistence;

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
        services.AddIdentityCore<IdentityUser>()
            .AddRoles<IdentityRole>()                          // enables RoleManager<IdentityRole>
            .AddEntityFrameworkStores<AppDbContext>();

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
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                };
            });

        services.AddAuthorization();

        // ── JWT Token Service ──
        services.AddSingleton<JwtTokenService>();

        // ── Dapper (Queries: SELECT) ──
        services.AddSingleton(new DapperConnectionFactory(connectionString));

        // ── FluentValidation ──
        services.AddValidatorsFromAssembly(typeof(ServiceCollectionExtensions).Assembly);

        return services;
    }
}

