namespace SamplVSSkill.Features.Specialties.ToggleSpecialtyStatus;

public static class ToggleSpecialtyStatusEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapPatch("/api/specialties/{id:int}/toggle-status", Handle)
           .WithTags("Specialties")
           .WithName("ToggleSpecialtyStatus")
           .Produces<ToggleSpecialtyStatusResponse>()
           .Produces(StatusCodes.Status404NotFound)
           .Produces(StatusCodes.Status401Unauthorized);
           //.RequireAuthorization();

    private static async Task<IResult> Handle(
        int id,
        ToggleSpecialtyStatusCommandHandler handler,
        CancellationToken ct)
    {
        var response = await handler.HandleAsync(id, ct);
        return response is not null ? Results.Ok(response) : Results.NotFound();
    }
}
