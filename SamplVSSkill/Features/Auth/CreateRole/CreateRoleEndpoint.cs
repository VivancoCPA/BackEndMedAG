using SamplVSSkill.Infrastructure.Middleware;

namespace SamplVSSkill.Features.Auth.CreateRole;

public static class CreateRoleEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapPost("/api/roles", Handle)
           .AddEndpointFilter<ValidationFilter<CreateRoleCommand>>()
           .WithTags("Roles")
           .WithName("CreateRole")
           .Produces<CreateRoleResponse>(StatusCodes.Status201Created)
           .Produces(StatusCodes.Status409Conflict)
           .ProducesValidationProblem()
           .Produces(StatusCodes.Status401Unauthorized)
           .RequireAuthorization();

    private static async Task<IResult> Handle(
        CreateRoleCommand command,
        CreateRoleCommandHandler handler,
        CancellationToken ct) =>
        await handler.HandleAsync(command, ct);
}
