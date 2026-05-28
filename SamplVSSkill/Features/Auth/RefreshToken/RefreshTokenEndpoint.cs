using SamplVSSkill.Infrastructure.Middleware;

namespace SamplVSSkill.Features.Auth.RefreshToken;

public static class RefreshTokenEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapPost("/api/auth/refresh", Handle)
           .AddEndpointFilter<ValidationFilter<RefreshTokenCommand>>()
           .WithTags("Auth")
           .WithName("RefreshToken")
           .Produces<RefreshTokenResponse>()
           .ProducesValidationProblem()
           .Produces(StatusCodes.Status400BadRequest)
           .AllowAnonymous();

    private static async Task<IResult> Handle(
        RefreshTokenCommand command,
        RefreshTokenCommandHandler handler,
        CancellationToken ct)
        => await handler.HandleAsync(command, ct);
}
