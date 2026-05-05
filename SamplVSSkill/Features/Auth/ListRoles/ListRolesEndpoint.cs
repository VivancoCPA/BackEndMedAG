namespace SamplVSSkill.Features.Auth.ListRoles;

public static class ListRolesEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/api/roles", Handle)
           .WithTags("Roles")
           .WithName("ListRoles")
           .Produces<IEnumerable<ListRolesResponse>>()
           .Produces(StatusCodes.Status401Unauthorized);
           //.RequireAuthorization(); // público — el frontend lo usa para mostrar roles en dropdowns

    private static async Task<IResult> Handle(
        ListRolesQueryHandler handler,
        CancellationToken ct)
    {
        var roles = await handler.HandleAsync(ct);
        return Results.Ok(roles);
    }
}
