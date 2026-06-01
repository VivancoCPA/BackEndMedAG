using FluentValidation;
using Microsoft.AspNetCore.Identity;
using SamplVSSkill.Domain.Entities;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SamplVSSkill.Features.Auth.UpdateUser;

// ── Request / Response ──────────────────────────────────────────
public record UpdateUserCommand(
    string Name,
    string LastName,
    string? DateOfBirth, // Recibido como string para evitar inconsistencias de formato por cultura/región
    string? PhoneNumber,
    string? PhotoUrl,
    string? Address);

public record UpdateUserResponse(
    string Id,
    string Email,
    string Name,
    string LastName,
    DateTime? DateOfBirth,
    string? PhoneNumber,
    string? PhotoUrl,
    string? Address);

// ── Validator ───────────────────────────────────────────────────
public class UpdateUserValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);

        RuleFor(x => x.DateOfBirth)
            .Must(dob => string.IsNullOrEmpty(dob) || DateTime.TryParseExact(dob, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out _))
            .WithMessage("La fecha de nacimiento debe estar en formato yyyy-MM-dd.");
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

        // Parsear fecha de nacimiento de forma independiente a la cultura del servidor
        DateTime? dateOfBirth = null;
        if (!string.IsNullOrWhiteSpace(command.DateOfBirth) &&
            DateTime.TryParseExact(command.DateOfBirth, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var parsedDate))
        {
            dateOfBirth = DateTime.SpecifyKind(parsedDate, DateTimeKind.Utc);
        }

        user.Name        = command.Name;
        user.LastName    = command.LastName;
        user.DateOfBirth = dateOfBirth;
        user.PhoneNumber = command.PhoneNumber;
        user.PhotoUrl    = command.PhotoUrl;
        user.Address     = command.Address;

        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            var errors = result.Errors.ToDictionary(e => e.Code, e => new[] { e.Description });
            return Results.ValidationProblem(errors);
        }

        return Results.Ok(new UpdateUserResponse(
            user.Id, user.Email!, user.Name, user.LastName,
            user.DateOfBirth, user.PhoneNumber, user.PhotoUrl, user.Address));
    }
}

