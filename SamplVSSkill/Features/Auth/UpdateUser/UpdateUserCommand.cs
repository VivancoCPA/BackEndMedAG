using FluentValidation;
using Microsoft.AspNetCore.Identity;
using SamplVSSkill.Domain.Entities;

namespace SamplVSSkill.Features.Auth.UpdateUser;

// ── Request / Response ──────────────────────────────────────────
public record UpdateUserCommand(
    string Name,
    string LastName,
    DateTime? DateOfBirth,
    Guid? InsurerId,
    string? PhoneNumber);

public record UpdateUserResponse(
    string Id,
    string Email,
    string Name,
    string LastName,
    DateTime? DateOfBirth,
    Guid? InsurerId,
    string? PhoneNumber);

// ── Validator ───────────────────────────────────────────────────
public class UpdateUserValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
    }
}

// ── Command Handler ─────────────────────────────────────────────
public class UpdateUserCommandHandler
{
    private readonly UserManager<AppUser> _userManager;

    public UpdateUserCommandHandler(UserManager<AppUser> userManager) => _userManager = userManager;

    public async Task<IResult> HandleAsync(string userId, UpdateUserCommand command, CancellationToken ct)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return Results.NotFound($"Usuario '{userId}' no encontrado.");

        user.Name        = command.Name;
        user.LastName    = command.LastName;
        user.DateOfBirth = command.DateOfBirth;
        user.InsurerId   = command.InsurerId;
        user.PhoneNumber = command.PhoneNumber;

        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            var errors = result.Errors.ToDictionary(e => e.Code, e => new[] { e.Description });
            return Results.ValidationProblem(errors);
        }

        return Results.Ok(new UpdateUserResponse(
            user.Id, user.Email!, user.Name, user.LastName,
            user.DateOfBirth, user.InsurerId, user.PhoneNumber));
    }
}
