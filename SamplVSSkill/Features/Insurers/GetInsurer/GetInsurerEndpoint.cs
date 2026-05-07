namespace SamplVSSkill.Features.Insurers.GetInsurer;

public static class GetInsurerEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/api/insurers/{id:guid}", Handle)
           .WithTags("Insurers")
           .WithName("GetInsurer")
           .Produces<GetInsurerResponse>()
           .Produces(StatusCodes.Status404NotFound)
           .Produces(StatusCodes.Status401Unauthorized)
           .RequireAuthorization();

    private static async Task<IResult> Handle(
        Guid id,
        GetInsurerQueryHandler handler,
        CancellationToken ct)
    {
        var response = await handler.HandleAsync(id, ct);
        return response is not null ? Results.Ok(response) : Results.NotFound();
    }
}
