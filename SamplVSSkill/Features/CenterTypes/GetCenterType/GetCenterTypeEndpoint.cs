namespace SamplVSSkill.Features.CenterTypes.GetCenterType;

public static class GetCenterTypeEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/api/center-types/{id:int}", Handle)
           .WithTags("Center Types")
           .WithName("GetCenterType")
           .Produces<GetCenterTypeResponse>()
           .Produces(StatusCodes.Status404NotFound);
           // Public — used to display type info in forms

    private static async Task<IResult> Handle(
        int id,
        GetCenterTypeQueryHandler handler,
        CancellationToken ct)
    {
        var response = await handler.HandleAsync(id, ct);
        return response is not null ? Results.Ok(response) : Results.NotFound();
    }
}
