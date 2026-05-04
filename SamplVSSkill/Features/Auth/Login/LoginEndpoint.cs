using SamplVSSkill.Infrastructure.Middleware;

namespace SamplVSSkill.Features.Auth.Login;

public static class LoginEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapPost("/api/auth/login", Handle)
           .AddEndpointFilter<ValidationFilter<LoginCommand>>()
           .WithTags("Auth")
           .WithName("Login")
           .Produces<LoginResponse>()
           .ProducesValidationProblem()
           .Produces(StatusCodes.Status401Unauthorized)
           .AllowAnonymous();

    private static async Task<IResult> Handle(
        LoginCommand command,
        LoginCommandHandler handler,
        CancellationToken ct)
    {
        return await handler.HandleAsync(command, ct);
    }
}
