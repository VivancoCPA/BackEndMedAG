namespace SamplVSSkill.Features.Auth.UpdateUser;

public static class UpdateUserEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapPut("/api/auth/users/{id}", Handle)
           .WithTags("Users")
           .WithName("UpdateUser")
           .Produces<UpdateUserResponse>()
           .ProducesValidationProblem()
           .Produces(StatusCodes.Status404NotFound)
           .Produces(StatusCodes.Status401Unauthorized);
           //.RequireAuthorization();

    private static async Task<IResult> Handle(
        string id,
        UpdateUserCommand command,
        UpdateUserCommandHandler handler,
        CancellationToken ct)
        => await handler.HandleAsync(id, command, ct);
}
