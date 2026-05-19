namespace SamplVSSkill.Features.Auth.ChangePassword;

public static class ChangePasswordEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapPost("/api/auth/change-password", Handle)
           .WithTags("Auth")
           .WithName("ChangePassword")
           .Produces<ChangePasswordResponse>()
           .ProducesValidationProblem()
           .Produces(StatusCodes.Status404NotFound);
           //.RequireAuthorization();

    private static async Task<IResult> Handle(
        ChangePasswordCommand command,
        ChangePasswordCommandHandler handler,
        CancellationToken ct)
        => await handler.HandleAsync(command, ct);
}
