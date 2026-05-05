namespace SamplVSSkill.Features.Auth.RemoveRole;

public static class RemoveRoleEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapDelete("/api/users/{userId}/roles/{roleName}", Handle)
           .WithTags("Users")
           .WithName("RemoveRole")
           .Produces<RemoveRoleResponse>()
           .Produces(StatusCodes.Status404NotFound)
           .Produces(StatusCodes.Status401Unauthorized)
           .RequireAuthorization();

    private static async Task<IResult> Handle(
        string userId,
        string roleName,
        RemoveRoleCommandHandler handler,
        CancellationToken ct) =>
        await handler.HandleAsync(userId, roleName, ct);
}
