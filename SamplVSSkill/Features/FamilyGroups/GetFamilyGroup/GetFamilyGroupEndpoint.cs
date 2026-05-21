namespace SamplVSSkill.Features.FamilyGroups.GetFamilyGroup;

public static class GetFamilyGroupEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/api/family-groups/{id:guid}", Handle)
           .WithTags("Family Groups")
           .WithName("GetFamilyGroup")
           .Produces<GetFamilyGroupResponse>()
           .Produces(StatusCodes.Status404NotFound);
           //.RequireAuthorization();

    private static async Task<IResult> Handle(
        Guid id,
        GetFamilyGroupQueryHandler handler,
        CancellationToken ct)
        => await handler.HandleAsync(id, ct);
}
