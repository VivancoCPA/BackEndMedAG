namespace SamplVSSkill.Features.Insurers.ListInsurers;

public static class ListInsurersEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/api/insurers", Handle)
           .WithTags("Insurers")
           .WithName("ListInsurers")
           .Produces<IEnumerable<ListInsurersResponse>>()
           .Produces(StatusCodes.Status401Unauthorized)
           .RequireAuthorization();

    private static async Task<IResult> Handle(
        ListInsurersQueryHandler handler,
        CancellationToken ct)
    {
        var items = await handler.HandleAsync(ct);
        return Results.Ok(items);
    }
}
