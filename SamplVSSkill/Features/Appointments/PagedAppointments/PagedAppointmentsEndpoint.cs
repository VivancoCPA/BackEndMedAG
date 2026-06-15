using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using SamplVSSkill.Domain.Common;
using System;
using System.Security.Claims;

namespace SamplVSSkill.Features.Appointments.PagedAppointments;

public static class PagedAppointmentsEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/api/appointments/paged", Handle)
           .WithTags("Appointments")
           .WithName("PagedAppointments")
           .Produces<PaginatedResult<PagedAppointmentItem>>(StatusCodes.Status200OK)
           .Produces(StatusCodes.Status401Unauthorized)
           .RequireAuthorization();

    private static async Task<IResult> Handle(
        ClaimsPrincipal principal,
        [FromQuery] int page,
        [FromQuery] int pageSize,
        [FromQuery] string? statusId,
        [FromQuery] DateTime? date,
        PagedAppointmentsQueryHandler handler,
        CancellationToken ct)
    {
        var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? principal.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Results.Unauthorized();
        }

        var queryParams = new PagedAppointmentsParams(page, pageSize, statusId, date);
        var results = await handler.HandleAsync(queryParams, userId, ct);
        return Results.Ok(results);
    }
}
