using SamplVSSkill.Infrastructure.Middleware;

namespace SamplVSSkill.Features.Auth.Register;

public static class RegisterEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapPost("/api/auth/register", Handle)
           .AddEndpointFilter<ValidationFilter<RegisterCommand>>()
           .WithTags("Auth")
           .WithName("Register")
           .Produces<RegisterResponse>(StatusCodes.Status201Created)
           .ProducesValidationProblem()
           .AllowAnonymous();

    private static async Task<IResult> Handle(
        RegisterCommand command,
        RegisterCommandHandler handler,
        CancellationToken ct)
    {
        return await handler.HandleAsync(command, ct);
    }
}
