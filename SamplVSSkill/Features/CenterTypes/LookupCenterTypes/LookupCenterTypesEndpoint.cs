using SamplVSSkill.Domain.Common;

namespace SamplVSSkill.Features.CenterTypes.LookupCenterTypes;

public static class LookupCenterTypesEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/api/center-types/lookup", Handle)
           .WithTags("Center Types")
           .WithName("LookupCenterTypes")
           .Produces<IEnumerable<LookupItem>>();
           // Public — used by forms to populate center type dropdowns

    private static async Task<IResult> Handle(
        LookupCenterTypesQueryHandler handler,
        CancellationToken ct)
    {
        var items = await handler.HandleAsync(ct);
        return Results.Ok(items);
    }
}
