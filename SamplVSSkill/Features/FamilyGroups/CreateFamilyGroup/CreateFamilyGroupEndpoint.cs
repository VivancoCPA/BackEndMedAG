namespace SamplVSSkill.Features.FamilyGroups.CreateFamilyGroup;

public static class CreateFamilyGroupEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapPost("/api/family-groups", Handle)
           .WithTags("Family Groups")
           .WithName("CreateFamilyGroup")
           .Produces<CreateFamilyGroupResponse>(StatusCodes.Status201Created)
           .ProducesValidationProblem();
           //.RequireAuthorization();

    private static async Task<IResult> Handle(
        CreateFamilyGroupCommand command,
        CreateFamilyGroupCommandHandler handler,
        CancellationToken ct)
        => await handler.HandleAsync(command, ct);
}
