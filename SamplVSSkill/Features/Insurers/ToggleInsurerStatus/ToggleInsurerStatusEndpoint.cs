namespace SamplVSSkill.Features.Insurers.ToggleInsurerStatus;

public static class ToggleInsurerStatusEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapPatch("/api/insurers/{id:guid}/toggle-status", Handle)
           .WithTags("Insurers")
           .WithName("ToggleInsurerStatus")
           .Produces<ToggleInsurerStatusResponse>()
           .Produces(StatusCodes.Status404NotFound)
           .Produces(StatusCodes.Status401Unauthorized)
           .RequireAuthorization();

    private static async Task<IResult> Handle(
        Guid id,
        ToggleInsurerStatusCommandHandler handler,
        CancellationToken ct)
    {
        var response = await handler.HandleAsync(id, ct);
        return response is not null ? Results.Ok(response) : Results.NotFound();
    }
}
