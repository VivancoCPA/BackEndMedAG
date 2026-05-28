namespace SamplVSSkill.Features.FamilyMemberships.ListFamilyMemberships;

public static class ListFamilyMembershipsEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/api/family-groups/{familyGroupId:guid}/members", Handle)
           .WithTags("Family Memberships")
           .WithName("ListFamilyMemberships")
           .Produces<IEnumerable<FamilyMembershipItem>>()
           .Produces(StatusCodes.Status404NotFound);
           //.RequireAuthorization();

    private static async Task<IResult> Handle(
        Guid familyGroupId,
        ListFamilyMembershipsQueryHandler handler,
        CancellationToken ct)
        => await handler.HandleAsync(familyGroupId, ct);
}
