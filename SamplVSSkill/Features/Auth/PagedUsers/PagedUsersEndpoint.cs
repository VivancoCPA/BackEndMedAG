using SamplVSSkill.Domain.Common;

namespace SamplVSSkill.Features.Auth.PagedUsers;

public static class PagedUsersEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/api/auth/users/paged", Handle)
           .WithTags("Users")
           .WithName("PagedUsers")
           .Produces<PaginatedResult<PagedUserItem>>()
           .Produces(StatusCodes.Status401Unauthorized);
           //.RequireAuthorization();

    private static async Task<IResult> Handle(
        [AsParameters] PagedUsersParams queryParams,
        PagedUsersQueryHandler handler,
        CancellationToken ct)
    {
        var result = await handler.HandleAsync(queryParams, ct);
        return Results.Ok(result);
    }
}
