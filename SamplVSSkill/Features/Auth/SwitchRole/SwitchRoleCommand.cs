using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using SamplVSSkill.Domain.Entities;
using SamplVSSkill.Infrastructure.Auth;

namespace SamplVSSkill.Features.Auth.SwitchRole;

public record SwitchRoleCommand(string RoleName);
public record SwitchRoleResponse(string Token);

public class SwitchRoleValidator : AbstractValidator<SwitchRoleCommand>
{
    public SwitchRoleValidator()
    {
        RuleFor(x => x.RoleName)
            .NotEmpty().WithMessage("El nombre del rol es requerido.");
    }
}

public class SwitchRoleCommandHandler
{
    private readonly UserManager<AppUser> _userManager;
    private readonly JwtTokenService _jwtService;

    public SwitchRoleCommandHandler(UserManager<AppUser> userManager, JwtTokenService jwtService)
    {
        _userManager = userManager;
        _jwtService = jwtService;
    }

    public async Task<IResult> HandleAsync(SwitchRoleCommand command, string userId, CancellationToken ct)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return Results.NotFound("Usuario no encontrado.");
        }

        // Verificar si el usuario está bloqueado
        if (user.LockoutEnd != null && user.LockoutEnd > DateTimeOffset.UtcNow)
        {
            return Results.Problem(
                title: "Usuario bloqueado",
                detail: "Tu cuenta se encuentra bloqueada. Contacta al administrador.",
                statusCode: StatusCodes.Status403Forbidden);
        }

        // Obtener los roles asignados en la base de datos
        var dbRoles = await _userManager.GetRolesAsync(user);

        // Validar que el usuario posea el rol solicitado
        if (!dbRoles.Contains(command.RoleName))
        {
            return Results.Problem(
                title: "Acceso denegado",
                detail: $"El usuario no tiene asignado el rol '{command.RoleName}'.",
                statusCode: StatusCodes.Status403Forbidden);
        }

        // Generar un nuevo JWT que contenga únicamente el rol activo seleccionado
        var token = _jwtService.GenerateToken(user, new[] { command.RoleName });

        return Results.Ok(new SwitchRoleResponse(token));
    }
}
