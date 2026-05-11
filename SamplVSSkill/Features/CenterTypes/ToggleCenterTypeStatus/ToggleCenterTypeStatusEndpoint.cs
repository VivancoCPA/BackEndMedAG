namespace SamplVSSkill.Features.CenterTypes.ToggleCenterTypeStatus;

public static class ToggleCenterTypeStatusEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapPatch("/api/center-types/{id:int}/toggle-status", Handle)
           .WithTags("Center Types")
           .WithName("ToggleCenterTypeStatus")
           .Produces<ToggleCenterTypeStatusResponse>()
           .Produces(StatusCodes.Status404NotFound)
           .Produces(StatusCodes.Status401Unauthorized);
           //.RequireAuthorization();

    private static async Task<IResult> Handle(
        int id,
        ToggleCenterTypeStatusCommandHandler handler,
        CancellationToken ct)
    {
        var response = await handler.HandleAsync(id, ct);
        return response is not null ? Results.Ok(response) : Results.NotFound();
    }
}
