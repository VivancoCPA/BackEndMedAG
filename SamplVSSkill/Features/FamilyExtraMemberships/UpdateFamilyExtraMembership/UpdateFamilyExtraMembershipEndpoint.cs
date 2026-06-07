using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using SamplVSSkill.Infrastructure.Middleware;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SamplVSSkill.Features.FamilyExtraMemberships.UpdateFamilyExtraMembership;

public static class UpdateFamilyExtraMembershipEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapPut("/api/family-groups/{familyGroupId:guid}/extra-members/{id:int}", Handle)
           .AddEndpointFilter<ValidationFilter<UpdateFamilyExtraMembershipCommand>>()
           .WithTags("Family Extra Memberships")
           .WithName("UpdateFamilyExtraMembership")
           .Produces<UpdateFamilyExtraMembershipResponse>(StatusCodes.Status200OK)
           .ProducesValidationProblem()
           .Produces(StatusCodes.Status404NotFound)
           .Produces(StatusCodes.Status500InternalServerError)
           .DisableAntiforgery();
           //.RequireAuthorization();

    private static async Task<IResult> Handle(
        Guid familyGroupId,
        int id,
        [FromForm] UpdateFamilyExtraMembershipCommand command,
        UpdateFamilyExtraMembershipCommandHandler handler,
        CancellationToken ct)
        => await handler.HandleAsync(familyGroupId, id, command, ct);
}
