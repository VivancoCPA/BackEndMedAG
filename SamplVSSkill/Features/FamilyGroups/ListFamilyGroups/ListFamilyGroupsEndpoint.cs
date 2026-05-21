namespace SamplVSSkill.Features.FamilyGroups.ListFamilyGroups;

public static class ListFamilyGroupsEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/api/family-groups", Handle)
           .WithTags("Family Groups")
           .WithName("ListFamilyGroups")
           .Produces<IEnumerable<ListFamilyGroupsResponse>>();
           //.RequireAuthorization();

    private static async Task<IResult> Handle(
        ListFamilyGroupsQueryHandler handler,
        CancellationToken ct)
    {
        var items = await handler.HandleAsync(ct);
        return Results.Ok(items);
    }
}
