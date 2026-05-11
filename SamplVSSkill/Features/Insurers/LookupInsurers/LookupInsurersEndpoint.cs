using SamplVSSkill.Domain.Common;

namespace SamplVSSkill.Features.Insurers.LookupInsurers;

public static class LookupInsurersEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/api/insurers/lookup", Handle)
           .WithTags("Insurers")
           .WithName("LookupInsurers")
           .Produces<IEnumerable<LookupItemGuid>>();
           // Public — used by forms to populate insurer dropdowns (e.g., pet insurance)

    private static async Task<IResult> Handle(
        LookupInsurersQueryHandler handler,
        CancellationToken ct)
    {
        var items = await handler.HandleAsync(ct);
        return Results.Ok(items);
    }
}
