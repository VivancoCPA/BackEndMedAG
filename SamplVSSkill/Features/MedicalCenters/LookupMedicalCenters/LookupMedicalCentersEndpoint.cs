using SamplVSSkill.Domain.Common;

namespace SamplVSSkill.Features.MedicalCenters.LookupMedicalCenters;

public static class LookupMedicalCentersEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/api/medical-centers/lookup", Handle)
           .WithTags("Medical Centers")
           .WithName("LookupMedicalCenters")
           .Produces<IEnumerable<LookupItemGuid>>();

    private static async Task<IResult> Handle(
        LookupMedicalCentersQueryHandler handler,
        CancellationToken ct)
    {
        var items = await handler.HandleAsync(ct);
        return Results.Ok(items);
    }
}
