namespace SamplVSSkill.Features.Auth.GetUserRoles;

public static class GetUserRolesEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/api/users/{userId}/roles", Handle)
           .WithTags("Users")
           .WithName("GetUserRoles")
           .Produces<GetUserRolesResponse>()
           .Produces(StatusCodes.Status404NotFound)
           .Produces(StatusCodes.Status401Unauthorized)
           .RequireAuthorization();

    private static async Task<IResult> Handle(
        string userId,
        GetUserRolesQueryHandler handler,
        CancellationToken ct) =>
        await handler.HandleAsync(userId, ct);
}
