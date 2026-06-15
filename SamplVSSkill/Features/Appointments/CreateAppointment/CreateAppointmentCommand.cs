using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SamplVSSkill.Domain.Entities;
using SamplVSSkill.Infrastructure.Persistence;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SamplVSSkill.Features.Appointments.CreateAppointment;

public record CreateAppointmentCommand(
    Guid? CenterId,
    Guid? DoctorId,
    int? SpecialtieId,
    Guid? InsurerId,
    string Description,
    DateTime? AppointmentDate,
    string StatusId = "PENDIENTE");

public record CreateAppointmentResponse(
    Guid Id,
    string UserId,
    Guid? CenterId,
    Guid? DoctorId,
    int? SpecialtieId,
    Guid? InsurerId,
    string Description,
    DateTime? AppointmentDate,
    string StatusId,
    DateTime CreatedAt);

public class CreateAppointmentValidator : AbstractValidator<CreateAppointmentCommand>
{
    private static readonly string[] ValidStatuses = new[]
    {
        "PENDIENTE", "CONFIRMADA", "INASISTENCIA", "CANCELADA", "REPROGRAMADA", "ENCONSULTA", "FINALIZADA"
    };

    public CreateAppointmentValidator()
    {
        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("La descripción es requerida.")
            .MaximumLength(1000).WithMessage("La descripción no puede exceder 1000 caracteres.");

        RuleFor(x => x.StatusId)
            .NotEmpty().WithMessage("El estado de la cita es requerido.")
            .Must(status => ValidStatuses.Contains(status.ToUpper()))
            .WithMessage($"El estado de la cita no es válido. Valores permitidos: {string.Join(", ", ValidStatuses)}.");
    }
}

public class CreateAppointmentCommandHandler
{
    private readonly AppDbContext _db;

    public CreateAppointmentCommandHandler(AppDbContext db) => _db = db;

    public async Task<IResult> HandleAsync(
        CreateAppointmentCommand command, string userId, CancellationToken ct)
    {
        // 1. Validar existencia de entidades relacionadas si se especificaron
        if (command.CenterId.HasValue && !await _db.MedicalCenters.AnyAsync(mc => mc.Id == command.CenterId.Value, ct))
        {
            return Results.BadRequest($"Centro médico con ID '{command.CenterId.Value}' no encontrado.");
        }

        if (command.DoctorId.HasValue && !await _db.Doctors.AnyAsync(d => d.Id == command.DoctorId.Value, ct))
        {
            return Results.BadRequest($"Médico con ID '{command.DoctorId.Value}' no encontrado.");
        }

        if (command.SpecialtieId.HasValue && !await _db.Specialties.AnyAsync(s => s.Id == command.SpecialtieId.Value, ct))
        {
            return Results.BadRequest($"Especialidad con ID '{command.SpecialtieId.Value}' no encontrada.");
        }

        if (command.InsurerId.HasValue && !await _db.Insurers.AnyAsync(i => i.Id == command.InsurerId.Value, ct))
        {
            return Results.BadRequest($"Aseguradora con ID '{command.InsurerId.Value}' no encontrada.");
        }

        // 2. Crear la cita
        var appointment = new AppointmentUser
        {
            Id = Guid.CreateVersion7(),
            UserId = userId,
            CenterId = command.CenterId,
            DoctorId = command.DoctorId,
            SpecialtieId = command.SpecialtieId,
            InsurerId = command.InsurerId,
            Description = command.Description,
            AppointmentDate = command.AppointmentDate.HasValue ? DateTime.SpecifyKind(command.AppointmentDate.Value, DateTimeKind.Utc) : null,
            StatusId = command.StatusId.ToUpper(),
            CreatedAt = DateTime.UtcNow
        };

        _db.AppointmentUsers.Add(appointment);
        await _db.SaveChangesAsync(ct);

        return Results.Created(
            $"/api/appointments/{appointment.Id}",
            new CreateAppointmentResponse(
                appointment.Id,
                appointment.UserId,
                appointment.CenterId,
                appointment.DoctorId,
                appointment.SpecialtieId,
                appointment.InsurerId,
                appointment.Description,
                appointment.AppointmentDate,
                appointment.StatusId,
                appointment.CreatedAt));
    }
}
