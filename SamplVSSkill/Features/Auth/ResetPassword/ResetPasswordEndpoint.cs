namespace SamplVSSkill.Features.Auth.ResetPassword;

public static class ResetPasswordEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapPost("/api/auth/reset-password", Handle)
           .WithTags("Auth")
           .WithName("ResetPassword")
           .Produces<ResetPasswordResponse>()
           .ProducesValidationProblem()
           .Produces(StatusCodes.Status400BadRequest);

    private static async Task<IResult> Handle(
        ResetPasswordCommand command,
        ResetPasswordCommandHandler handler,
        CancellationToken ct)
        => await handler.HandleAsync(command, ct);
}
