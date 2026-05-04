namespace SamplVSSkill.Features.MedicalCenters.GetMedicalCenter;

public static class GetMedicalCenterEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/api/medical-centers/{id:guid}", Handle)
           .WithTags("MedicalCenters")
           .WithName("GetMedicalCenter")
           .Produces<GetMedicalCenterResponse>()
           .Produces(StatusCodes.Status404NotFound)
           .Produces(StatusCodes.Status401Unauthorized)
           .RequireAuthorization();

    private static async Task<IResult> Handle(
        Guid id,
        GetMedicalCenterQueryHandler handler,
        CancellationToken ct)
    {
        var response = await handler.HandleAsync(id, ct);
        return response is not null
            ? Results.Ok(response)
            : Results.NotFound();
    }
}
