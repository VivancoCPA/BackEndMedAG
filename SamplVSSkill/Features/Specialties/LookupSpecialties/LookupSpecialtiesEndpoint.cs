using SamplVSSkill.Domain.Common;

namespace SamplVSSkill.Features.Specialties.LookupSpecialties;

public static class LookupSpecialtiesEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/api/specialties/lookup", Handle)
           .WithTags("Specialties")
           .WithName("LookupSpecialties")
           .Produces<IEnumerable<LookupItem>>();
           // Public — used by forms to populate specialty dropdowns (e.g., doctor registration)

    private static async Task<IResult> Handle(
        LookupSpecialtiesQueryHandler handler,
        CancellationToken ct)
    {
        var items = await handler.HandleAsync(ct);
        return Results.Ok(items);
    }
}
