namespace SamplVSSkill.Features.Auth.DeleteRole;

public static class DeleteRoleEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapDelete("/api/roles/{roleName}", Handle)
           .WithTags("Roles")
           .WithName("DeleteRole")
           .Produces<DeleteRoleResponse>()
           .Produces(StatusCodes.Status404NotFound)
           .ProducesValidationProblem()
           .Produces(StatusCodes.Status401Unauthorized)
           .RequireAuthorization();

    private static async Task<IResult> Handle(
        string roleName,
        DeleteRoleCommandHandler handler,
        CancellationToken ct) =>
        await handler.HandleAsync(roleName, ct);
}
