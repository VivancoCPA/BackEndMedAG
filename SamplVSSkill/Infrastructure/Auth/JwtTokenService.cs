using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace SamplVSSkill.Infrastructure.Auth;

/// <summary>
/// Generates JWT tokens for authenticated users.
/// Registered as Singleton — only depends on configuration, not on scoped services.
/// </summary>
public sealed class JwtTokenService
{
    private readonly string _issuer;
    private readonly string _audience;
    private readonly byte[] _key;
    private readonly int _expirationMinutes;

    public JwtTokenService(IConfiguration configuration)
    {
        var jwtSection = configuration.GetSection("Jwt");
        _issuer = jwtSection["Issuer"] ?? throw new InvalidOperationException("Jwt:Issuer not configured.");
        _audience = jwtSection["Audience"] ?? throw new InvalidOperationException("Jwt:Audience not configured.");
        _key = Encoding.UTF8.GetBytes(
            jwtSection["Key"] ?? throw new InvalidOperationException("Jwt:Key not configured."));
        _expirationMinutes = int.Parse(jwtSection["ExpirationMinutes"] ?? "60");
    }

    public string GenerateToken(IdentityUser user)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(_key),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_expirationMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
