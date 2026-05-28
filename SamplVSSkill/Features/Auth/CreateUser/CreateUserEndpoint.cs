using SamplVSSkill.Infrastructure.Middleware;

namespace SamplVSSkill.Features.Auth.CreateUser;

public static class CreateUserEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapPost("/api/auth/users", Handle)
           .AddEndpointFilter<ValidationFilter<CreateUserCommand>>()
           .WithTags("Users")
           .WithName("CreateUser")
           .Produces<CreateUserResponse>(StatusCodes.Status201Created)
           .ProducesValidationProblem()
           .Produces(StatusCodes.Status400BadRequest)
           .Produces(StatusCodes.Status409Conflict);
           //.RequireAuthorization(); // Omitido por desarrollo, se puede proteger con Roles/Admins luego.

    private static async Task<IResult> Handle(
        CreateUserCommand command,
        CreateUserCommandHandler handler,
        CancellationToken ct)
        => await handler.HandleAsync(command, ct);
}
