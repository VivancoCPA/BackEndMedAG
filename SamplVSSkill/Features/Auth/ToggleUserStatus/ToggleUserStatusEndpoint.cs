namespace SamplVSSkill.Features.Auth.ToggleUserStatus;

public static class ToggleUserStatusEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapPatch("/api/users/{userId}/toggle-status", Handle)
           .WithTags("Users")
           .WithName("ToggleUserStatus")
           .Produces<ToggleUserStatusResponse>()
           .Produces(StatusCodes.Status404NotFound)
           .Produces(StatusCodes.Status401Unauthorized)
           .RequireAuthorization();

    private static async Task<IResult> Handle(
        string userId,
        ToggleUserStatusCommandHandler handler,
        CancellationToken ct) =>
        await handler.HandleAsync(userId, ct);
}
