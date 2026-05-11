namespace SamplVSSkill.Features.CenterTypes.ListCenterTypes;

public static class ListCenterTypesEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/api/center-types", Handle)
           .WithTags("Center Types")
           .WithName("ListCenterTypes")
           .Produces<IEnumerable<ListCenterTypesResponse>>();
           // Public — used to populate dropdowns in medical center forms

    private static async Task<IResult> Handle(
        ListCenterTypesQueryHandler handler,
        CancellationToken ct)
    {
        var items = await handler.HandleAsync(ct);
        return Results.Ok(items);
    }
}
