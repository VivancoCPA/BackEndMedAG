namespace SamplVSSkill.Features.MedicalCenters.DeleteMedicalCenter;

public static class DeleteMedicalCenterEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        // PATCH — toggles IsActive between true/false (deactivate or reactivate)
        app.MapPatch("/api/medical-centers/{id:guid}/toggle-status", Handle)
           .WithTags("MedicalCenters")
           .WithName("ToggleMedicalCenterStatus")
           .Produces<ToggleMedicalCenterStatusResponse>()
           .Produces(StatusCodes.Status404NotFound)
           .Produces(StatusCodes.Status401Unauthorized);
           //.RequireAuthorization();

    private static async Task<IResult> Handle(
        Guid id,
        DeleteMedicalCenterCommandHandler handler,
        CancellationToken ct)
    {
        var response = await handler.HandleAsync(id, ct);
        return response is not null
            ? Results.Ok(response)
            : Results.NotFound();
    }
}
