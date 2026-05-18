using SamplVSSkill.Domain.Common;

namespace SamplVSSkill.Features.Doctors.LookupDoctors;

public static class LookupDoctorsEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/api/doctors/lookup", Handle)
           .WithTags("Doctors")
           .WithName("LookupDoctors")
           .Produces<IEnumerable<LookupItemGuid>>();

    private static async Task<IResult> Handle(
        LookupDoctorsQueryHandler handler,
        CancellationToken ct)
    {
        var items = await handler.HandleAsync(ct);
        return Results.Ok(items);
    }
}
