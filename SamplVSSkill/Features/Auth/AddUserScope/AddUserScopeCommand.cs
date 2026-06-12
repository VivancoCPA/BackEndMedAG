using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SamplVSSkill.Domain.Entities;
using SamplVSSkill.Infrastructure.Persistence;

namespace SamplVSSkill.Features.Auth.AddUserScope;

public record AddUserScopeCommand(string UserIdAdmin, string UserId);

public record AddUserScopeResponse(int Id, string UserIdAdmin, string UserId);

public class AddUserScopeValidator : AbstractValidator<AddUserScopeCommand>
{
    public AddUserScopeValidator()
    {
        RuleFor(x => x.UserIdAdmin).NotEmpty().WithMessage("El ID del administrador es requerido.");
        RuleFor(x => x.UserId).NotEmpty().WithMessage("El ID del usuario es requerido.");
    }
}

public class AddUserScopeCommandHandler
{
    private readonly AppDbContext _db;

    public AddUserScopeCommandHandler(AppDbContext db) => _db = db;

    public async Task<IResult> HandleAsync(AddUserScopeCommand command, CancellationToken ct)
    {
        // Verificar si ya existe esta relación en user_scope
        var exists = await _db.UserScopes.AnyAsync(
            x => x.UserIdAdmin == command.UserIdAdmin && x.UserId == command.UserId, ct);

        if (exists)
        {
            return Results.Conflict("El usuario ya se encuentra en el ámbito del administrador.");
        }

        // Verificar si existen los usuarios
        var adminExists = await _db.Users.AnyAsync(x => x.Id == command.UserIdAdmin, ct);
        if (!adminExists)
        {
            return Results.NotFound($"Administrador con ID '{command.UserIdAdmin}' no encontrado.");
        }

        var userExists = await _db.Users.AnyAsync(x => x.Id == command.UserId, ct);
        if (!userExists)
        {
            return Results.NotFound($"Usuario con ID '{command.UserId}' no encontrado.");
        }

        var userScope = new UserScope
        {
            UserIdAdmin = command.UserIdAdmin,
            UserId = command.UserId
        };

        _db.UserScopes.Add(userScope);
        await _db.SaveChangesAsync(ct);

        return Results.Created(
            $"/api/users/{command.UserIdAdmin}/scope/{command.UserId}",
            new AddUserScopeResponse(userScope.Id, userScope.UserIdAdmin!, userScope.UserId!));
    }
}
