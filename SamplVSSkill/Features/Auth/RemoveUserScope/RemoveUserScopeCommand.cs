using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SamplVSSkill.Infrastructure.Persistence;

namespace SamplVSSkill.Features.Auth.RemoveUserScope;

public record RemoveUserScopeCommand(string UserIdAdmin, string UserId);

public record RemoveUserScopeResponse(string Message);

public class RemoveUserScopeValidator : AbstractValidator<RemoveUserScopeCommand>
{
    public RemoveUserScopeValidator()
    {
        RuleFor(x => x.UserIdAdmin).NotEmpty().WithMessage("El ID del administrador es requerido.");
        RuleFor(x => x.UserId).NotEmpty().WithMessage("El ID del usuario es requerido.");
    }
}

public class RemoveUserScopeCommandHandler
{
    private readonly AppDbContext _db;

    public RemoveUserScopeCommandHandler(AppDbContext db) => _db = db;

    public async Task<IResult> HandleAsync(RemoveUserScopeCommand command, CancellationToken ct)
    {
        var userScope = await _db.UserScopes.FirstOrDefaultAsync(
            x => x.UserIdAdmin == command.UserIdAdmin && x.UserId == command.UserId, ct);

        if (userScope is null)
        {
            return Results.NotFound("No se encontró al usuario dentro del ámbito del administrador.");
        }

        _db.UserScopes.Remove(userScope);
        await _db.SaveChangesAsync(ct);

        return Results.Ok(new RemoveUserScopeResponse("Usuario eliminado del ámbito correctamente."));
    }
}
