using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SamplVSSkill.Features.FamilyExtraMemberships.RemoveFamilyExtraMembership;

public static class RemoveFamilyExtraMembershipEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapDelete("/api/family-groups/{familyGroupId:guid}/extra-members/{id:int}", Handle)
           .WithTags("Family Extra Memberships")
           .WithName("RemoveFamilyExtraMembership")
           .Produces<RemoveFamilyExtraMembershipResponse>(StatusCodes.Status200OK)
           .Produces(StatusCodes.Status404NotFound);
           //.RequireAuthorization();

    private static async Task<IResult> Handle(
        Guid familyGroupId,
        int id,
        RemoveFamilyExtraMembershipCommandHandler handler,
        CancellationToken ct)
        => await handler.HandleAsync(familyGroupId, id, ct);
}
