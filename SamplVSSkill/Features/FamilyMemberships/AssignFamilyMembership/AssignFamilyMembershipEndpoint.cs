using SamplVSSkill.Infrastructure.Middleware;

namespace SamplVSSkill.Features.FamilyMemberships.AssignFamilyMembership;

public static class AssignFamilyMembershipEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapPost("/api/family-groups/{familyGroupId:guid}/members", Handle)
           .AddEndpointFilter<ValidationFilter<AssignFamilyMembershipCommand>>()
           .WithTags("Family Memberships")
           .WithName("AssignFamilyMembership")
           .Produces<AssignFamilyMembershipResponse>(StatusCodes.Status201Created)
           .ProducesValidationProblem()
           .Produces(StatusCodes.Status404NotFound)
           .Produces(StatusCodes.Status409Conflict);
           //.RequireAuthorization();

    private static async Task<IResult> Handle(
        Guid familyGroupId,
        AssignFamilyMembershipCommand command,
        AssignFamilyMembershipCommandHandler handler,
        CancellationToken ct)
        => await handler.HandleAsync(familyGroupId, command, ct);
}
