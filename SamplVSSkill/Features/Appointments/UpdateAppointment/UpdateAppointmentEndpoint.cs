using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SamplVSSkill.Infrastructure.Middleware;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace SamplVSSkill.Features.Appointments.UpdateAppointment;

public static class UpdateAppointmentEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapPut("/api/appointments/{id:guid}", Handle)
           .DisableAntiforgery()
           .AddEndpointFilter<ValidationFilter<UpdateAppointmentCommand>>()
           .WithTags("Appointments")
           .WithName("UpdateAppointment")
           .Produces<UpdateAppointmentResponse>(StatusCodes.Status200OK)
           .ProducesValidationProblem()
           .Produces(StatusCodes.Status400BadRequest)
           .Produces(StatusCodes.Status401Unauthorized)
           .Produces(StatusCodes.Status403Forbidden)
           .Produces(StatusCodes.Status404NotFound)
           .RequireAuthorization();

    private static async Task<IResult> Handle(
        Guid id,
        ClaimsPrincipal principal,
        UpdateAppointmentCommand command,
        UpdateAppointmentCommandHandler handler,
        CancellationToken ct)
    {
        var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? principal.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Results.Unauthorized();
        }

        var isStaff = principal.IsInRole("SuperAdmin") || principal.IsInRole("Admin");

        return await handler.HandleAsync(id, command, userId, isStaff, ct);
    }
}
