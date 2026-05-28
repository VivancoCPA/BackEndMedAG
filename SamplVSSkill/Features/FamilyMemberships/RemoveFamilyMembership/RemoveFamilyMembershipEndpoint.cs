namespace SamplVSSkill.Features.FamilyMemberships.RemoveFamilyMembership;

public static class RemoveFamilyMembershipEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapDelete("/api/family-groups/{familyGroupId:guid}/members/{userId}", Handle)
           .WithTags("Family Memberships")
           .WithName("RemoveFamilyMembership")
           .Produces<RemoveFamilyMembershipResponse>()
           .Produces(StatusCodes.Status404NotFound);
           //.RequireAuthorization();

    private static async Task<IResult> Handle(
        Guid familyGroupId,
        string userId,
        RemoveFamilyMembershipCommandHandler handler,
        CancellationToken ct)
        => await handler.HandleAsync(familyGroupId, userId, ct);
}
