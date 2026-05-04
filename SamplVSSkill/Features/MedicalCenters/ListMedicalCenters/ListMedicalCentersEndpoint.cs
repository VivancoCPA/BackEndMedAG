namespace SamplVSSkill.Features.MedicalCenters.ListMedicalCenters;

public static class ListMedicalCentersEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/api/medical-centers", Handle)
           .WithTags("MedicalCenters")
           .WithName("ListMedicalCenters")
           .Produces<IEnumerable<ListMedicalCentersResponse>>()
           .Produces(StatusCodes.Status401Unauthorized);
           //.RequireAuthorization();

    private static async Task<IResult> Handle(
        ListMedicalCentersQueryHandler handler,
        CancellationToken ct)
    {
        var response = await handler.HandleAsync(ct);
        return Results.Ok(response);
    }
}
