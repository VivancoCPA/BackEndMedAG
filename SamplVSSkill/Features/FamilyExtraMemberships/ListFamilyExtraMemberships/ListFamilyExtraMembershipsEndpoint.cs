using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SamplVSSkill.Features.FamilyExtraMemberships.ListFamilyExtraMemberships;

public static class ListFamilyExtraMembershipsEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/api/family-groups/{familyGroupId:guid}/extra-members", Handle)
           .WithTags("Family Extra Memberships")
           .WithName("ListFamilyExtraMemberships")
           .Produces<IEnumerable<FamilyExtraMembershipItem>>(StatusCodes.Status200OK)
           .Produces(StatusCodes.Status404NotFound);
           //.RequireAuthorization();

    private static async Task<IResult> Handle(
        Guid familyGroupId,
        ListFamilyExtraMembershipsQueryHandler handler,
        CancellationToken ct)
        => await handler.HandleAsync(familyGroupId, ct);
}
