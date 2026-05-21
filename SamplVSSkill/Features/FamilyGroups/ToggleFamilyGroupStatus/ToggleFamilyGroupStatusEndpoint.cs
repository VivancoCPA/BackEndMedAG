namespace SamplVSSkill.Features.FamilyGroups.ToggleFamilyGroupStatus;

public static class ToggleFamilyGroupStatusEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapPatch("/api/family-groups/{id:guid}/toggle", Handle)
           .WithTags("Family Groups")
           .WithName("ToggleFamilyGroupStatus")
           .Produces<ToggleFamilyGroupStatusResponse>()
           .Produces(StatusCodes.Status404NotFound);
           //.RequireAuthorization();

    private static async Task<IResult> Handle(
        Guid id,
        ToggleFamilyGroupStatusCommandHandler handler,
        CancellationToken ct)
        => await handler.HandleAsync(id, ct);
}
