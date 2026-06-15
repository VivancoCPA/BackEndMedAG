using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System;
using System.Security.Claims;

namespace SamplVSSkill.Features.Appointments.ListAppointments;

public static class ListAppointmentsEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/api/appointments", Handle)
           .WithTags("Appointments")
           .WithName("ListAppointments")
           .Produces<IEnumerable<ListAppointmentsResponse>>(StatusCodes.Status200OK)
           .Produces(StatusCodes.Status401Unauthorized)
           .RequireAuthorization();

    private static async Task<IResult> Handle(
        ClaimsPrincipal principal,
        [FromQuery] string? statusId,
        [FromQuery] DateTime? date,
        ListAppointmentsQueryHandler handler,
        CancellationToken ct)
    {
        var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? principal.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Results.Unauthorized();
        }

        var results = await handler.HandleAsync(userId, statusId, date, ct);
        return Results.Ok(results);
    }
}
