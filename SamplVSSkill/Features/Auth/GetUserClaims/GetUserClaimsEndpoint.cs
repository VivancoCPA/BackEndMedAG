namespace SamplVSSkill.Features.Auth.GetUserClaims;

public static class GetUserClaimsEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/api/users/{userId}/claims", Handle)
           .WithTags("Users")
           .WithName("GetUserClaims")
           .Produces<GetUserClaimsResponse>()
           .Produces(StatusCodes.Status404NotFound)
           .Produces(StatusCodes.Status401Unauthorized)
           .RequireAuthorization();

    private static async Task<IResult> Handle(
        string userId,
        GetUserClaimsQueryHandler handler,
        CancellationToken ct) =>
        await handler.HandleAsync(userId, ct);
}
