using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SamplVSSkill.Domain.Entities;
using SamplVSSkill.Infrastructure.Persistence;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SamplVSSkill.Features.Appointments.UpdateAppointment;

public record UpdateAppointmentCommand(
    Guid? CenterId,
    Guid? DoctorId,
    int? SpecialtieId,
    Guid? InsurerId,
    string Description,
    DateTime? AppointmentDate,
    string StatusId);

public record UpdateAppointmentResponse(
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

public class UpdateAppointmentValidator : AbstractValidator<UpdateAppointmentCommand>
{
    private static readonly string[] ValidStatuses = new[]
    {
        "PENDIENTE", "CONFIRMADA", "INASISTENCIA", "CANCELADA", "REPROGRAMADA", "ENCONSULTA", "FINALIZADA"
    };

    public UpdateAppointmentValidator()
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

public class UpdateAppointmentCommandHandler
{
    private readonly AppDbContext _db;

    public UpdateAppointmentCommandHandler(AppDbContext db) => _db = db;

    public async Task<IResult> HandleAsync(
        Guid id, UpdateAppointmentCommand command, string userId, bool isStaff, CancellationToken ct)
    {
        var appointment = await _db.AppointmentUsers.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (appointment is null)
        {
            return Results.NotFound($"Cita con ID '{id}' no encontrada.");
        }

        // Validar propiedad de la cita (solo el dueño de la cita o administradores pueden modificarla)
        if (!isStaff && appointment.UserId != userId)
        {
            return Results.Forbid();
        }

        // Validar existencia de entidades relacionadas si se especificaron
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

        // Actualizar datos
        appointment.CenterId = command.CenterId;
        appointment.DoctorId = command.DoctorId;
        appointment.SpecialtieId = command.SpecialtieId;
        appointment.InsurerId = command.InsurerId;
        appointment.Description = command.Description;
        appointment.AppointmentDate = command.AppointmentDate.HasValue ? DateTime.SpecifyKind(command.AppointmentDate.Value, DateTimeKind.Utc) : null;
        appointment.StatusId = command.StatusId.ToUpper();

        await _db.SaveChangesAsync(ct);

        return Results.Ok(new UpdateAppointmentResponse(
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
