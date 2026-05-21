namespace SamplVSSkill.Features.FamilyGroups.UpdateFamilyGroup;

public static class UpdateFamilyGroupEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapPut("/api/family-groups/{id:guid}", Handle)
           .WithTags("Family Groups")
           .WithName("UpdateFamilyGroup")
           .Produces<UpdateFamilyGroupResponse>()
           .Produces(StatusCodes.Status404NotFound)
           .ProducesValidationProblem();
           //.RequireAuthorization();

    private static async Task<IResult> Handle(
        Guid id,
        UpdateFamilyGroupCommand command,
        UpdateFamilyGroupCommandHandler handler,
        CancellationToken ct)
        => await handler.HandleAsync(id, command, ct);
}
