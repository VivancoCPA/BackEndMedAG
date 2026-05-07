namespace SamplVSSkill.Features.MedicalCenters.SummaryMedicalCenters;

public static class SummaryMedicalCentersEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/api/medical-centers/summary", Handle)
           .WithTags("Medical Centers")
           .WithName("SummaryMedicalCenters")
           .Produces<MedicalCenterSummaryResponse>()
           .Produces(StatusCodes.Status401Unauthorized);
           //.RequireAuthorization();

    private static async Task<IResult> Handle(
        SummaryMedicalCentersQueryHandler handler,
        CancellationToken ct)
    {
        var summary = await handler.HandleAsync(ct);
        return Results.Ok(summary);
    }
}
