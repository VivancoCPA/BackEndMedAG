using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SamplVSSkill.Features.AppointmentStatuses.ListAppointmentStatuses;

public static class ListAppointmentStatusesEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/api/appointment-statuses", Handle)
           .WithTags("AppointmentStatuses")
           .WithName("ListAppointmentStatuses")
           .Produces<IEnumerable<AppointmentStatusResponse>>();

    private static async Task<IResult> Handle(
        ListAppointmentStatusesQueryHandler handler,
        CancellationToken ct)
    {
        var items = await handler.HandleAsync(ct);
        return Results.Ok(items);
    }
}
