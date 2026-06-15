using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SamplVSSkill.Infrastructure.Middleware;
using System.Security.Claims;

namespace SamplVSSkill.Features.Appointments.CreateAppointment;

public static class CreateAppointmentEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapPost("/api/appointments", Handle)
           .DisableAntiforgery()
           .AddEndpointFilter<ValidationFilter<CreateAppointmentCommand>>()
           .WithTags("Appointments")
           .WithName("CreateAppointment")
           .Produces<CreateAppointmentResponse>(StatusCodes.Status201Created)
           .ProducesValidationProblem()
           .Produces(StatusCodes.Status400BadRequest)
           .Produces(StatusCodes.Status401Unauthorized)
           .RequireAuthorization();

    private static async Task<IResult> Handle(
        ClaimsPrincipal principal,
        CreateAppointmentCommand command,
        CreateAppointmentCommandHandler handler,
        CancellationToken ct)
    {
        var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? principal.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Results.Unauthorized();
        }

        return await handler.HandleAsync(command, userId, ct);
    }
}
