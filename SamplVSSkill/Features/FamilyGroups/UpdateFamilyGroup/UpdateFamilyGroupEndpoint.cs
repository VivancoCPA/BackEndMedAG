using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace SamplVSSkill.Features.FamilyGroups.UpdateFamilyGroup;

public static class UpdateFamilyGroupEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapPut("/api/family-groups/{id:guid}", Handle)
           .WithTags("Family Groups")
           .WithName("UpdateFamilyGroup")
           .Produces<UpdateFamilyGroupResponse>()
           .Produces(StatusCodes.Status404NotFound)
           .ProducesValidationProblem()
           .DisableAntiforgery();
           //.RequireAuthorization();

    private static async Task<IResult> Handle(
        Guid id,
        [FromForm] UpdateFamilyGroupCommand command,
        UpdateFamilyGroupCommandHandler handler,
        CancellationToken ct)
        => await handler.HandleAsync(id, command, ct);
}
