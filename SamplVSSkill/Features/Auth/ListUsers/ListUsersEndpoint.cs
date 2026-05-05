namespace SamplVSSkill.Features.Auth.ListUsers;

public static class ListUsersEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/api/users", Handle)
           .WithTags("Users")
           .WithName("ListUsers")
           .Produces<IEnumerable<ListUsersResponse>>()
           .Produces(StatusCodes.Status401Unauthorized)
           .RequireAuthorization();

    private static async Task<IResult> Handle(
        ListUsersQueryHandler handler,
        CancellationToken ct)
    {
        var users = await handler.HandleAsync(ct);
        return Results.Ok(users);
    }
}
