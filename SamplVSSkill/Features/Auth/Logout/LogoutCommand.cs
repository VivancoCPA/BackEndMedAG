using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using SamplVSSkill.Domain.Entities;

namespace SamplVSSkill.Features.Auth.Logout;

public record LogoutResponse(string Message);

public class LogoutCommandHandler
{
    private readonly UserManager<AppUser> _userManager;

    public LogoutCommandHandler(UserManager<AppUser> userManager) =>
        _userManager = userManager;

    public async Task<IResult> HandleAsync(string userId, CancellationToken ct)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return Results.NotFound("Usuario no encontrado.");
        }

        // Invalidar el refresh token del usuario
        user.RefreshToken = null;
        user.RefreshTokenExpiryTime = null;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            return Results.Problem(
                title: "Error al cerrar sesión",
                detail: "No se pudo invalidar la sesión del usuario.",
                statusCode: StatusCodes.Status500InternalServerError);
        }

        return Results.Ok(new LogoutResponse("Sesión cerrada correctamente."));
    }
}
