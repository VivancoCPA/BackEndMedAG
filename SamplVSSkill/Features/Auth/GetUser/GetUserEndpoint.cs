namespace SamplVSSkill.Features.Auth.GetUser;

public static class GetUserEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/api/users/{userId}", Handle)
           .WithTags("Users")
           .WithName("GetUser")
           .Produces<GetUserResponse>()
           .Produces(StatusCodes.Status404NotFound)
           .Produces(StatusCodes.Status401Unauthorized)
           .RequireAuthorization();

    private static async Task<IResult> Handle(
        string userId,
        GetUserQueryHandler handler,
        CancellationToken ct) =>
        await handler.HandleAsync(userId, ct);
}
