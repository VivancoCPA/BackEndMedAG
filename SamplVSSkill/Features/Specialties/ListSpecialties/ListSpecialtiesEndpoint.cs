namespace SamplVSSkill.Features.Specialties.ListSpecialties;

public static class ListSpecialtiesEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/api/specialties", Handle)
           .WithTags("Specialties")
           .WithName("ListSpecialties")
           .Produces<IEnumerable<ListSpecialtiesResponse>>();
           // Public — frontend uses it to populate specialty dropdowns

    private static async Task<IResult> Handle(
        ListSpecialtiesQueryHandler handler,
        CancellationToken ct)
    {
        var items = await handler.HandleAsync(ct);
        return Results.Ok(items);
    }
}
