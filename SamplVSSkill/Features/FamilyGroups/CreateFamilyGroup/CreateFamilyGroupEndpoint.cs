using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace SamplVSSkill.Features.FamilyGroups.CreateFamilyGroup;

public static class CreateFamilyGroupEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapPost("/api/family-groups", Handle)
           .WithTags("Family Groups")
           .WithName("CreateFamilyGroup")
           .Produces<CreateFamilyGroupResponse>(StatusCodes.Status201Created)
           .ProducesValidationProblem()
           .DisableAntiforgery();
           //.RequireAuthorization();

    private static async Task<IResult> Handle(
        [FromForm] CreateFamilyGroupCommand command,
        CreateFamilyGroupCommandHandler handler,
        CancellationToken ct)
        => await handler.HandleAsync(command, ct);
}
