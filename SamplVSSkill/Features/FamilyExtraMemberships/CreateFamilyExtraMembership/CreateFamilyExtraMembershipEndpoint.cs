using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using SamplVSSkill.Infrastructure.Middleware;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SamplVSSkill.Features.FamilyExtraMemberships.CreateFamilyExtraMembership;

public static class CreateFamilyExtraMembershipEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapPost("/api/family-groups/{familyGroupId:guid}/extra-members", Handle)
           .AddEndpointFilter<ValidationFilter<CreateFamilyExtraMembershipCommand>>()
           .WithTags("Family Extra Memberships")
           .WithName("CreateFamilyExtraMembership")
           .Produces<CreateFamilyExtraMembershipResponse>(StatusCodes.Status201Created)
           .ProducesValidationProblem()
           .Produces(StatusCodes.Status404NotFound)
           .Produces(StatusCodes.Status500InternalServerError)
           .DisableAntiforgery();
           //.RequireAuthorization();

    private static async Task<IResult> Handle(
        Guid familyGroupId,
        [FromForm] CreateFamilyExtraMembershipCommand command,
        CreateFamilyExtraMembershipCommandHandler handler,
        CancellationToken ct)
        => await handler.HandleAsync(familyGroupId, command, ct);
}
