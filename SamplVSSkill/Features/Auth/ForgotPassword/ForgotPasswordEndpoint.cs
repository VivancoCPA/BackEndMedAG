namespace SamplVSSkill.Features.Auth.ForgotPassword;

public static class ForgotPasswordEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapPost("/api/auth/forgot-password", Handle)
           .WithTags("Auth")
           .WithName("ForgotPassword")
           .Produces<ForgotPasswordResponse>()
           .ProducesValidationProblem();

    private static async Task<IResult> Handle(
        ForgotPasswordCommand command,
        ForgotPasswordCommandHandler handler,
        CancellationToken ct)
        => await handler.HandleAsync(command, ct);
}
