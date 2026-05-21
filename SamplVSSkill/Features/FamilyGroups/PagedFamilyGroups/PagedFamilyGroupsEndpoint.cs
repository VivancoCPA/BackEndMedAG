using SamplVSSkill.Domain.Common;

namespace SamplVSSkill.Features.FamilyGroups.PagedFamilyGroups;

public static class PagedFamilyGroupsEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/api/family-groups/paged", Handle)
           .WithTags("Family Groups")
           .WithName("PagedFamilyGroups")
           .Produces<PaginatedResult<PagedFamilyGroupItem>>();
           //.RequireAuthorization();

    private static async Task<IResult> Handle(
        [AsParameters] PagedFamilyGroupsParams queryParams,
        PagedFamilyGroupsQueryHandler handler,
        CancellationToken ct)
    {
        var result = await handler.HandleAsync(queryParams, ct);
        return Results.Ok(result);
    }
}
