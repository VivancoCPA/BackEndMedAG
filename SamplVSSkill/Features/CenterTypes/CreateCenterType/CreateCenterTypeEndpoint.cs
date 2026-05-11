using SamplVSSkill.Infrastructure.Middleware;

namespace SamplVSSkill.Features.CenterTypes.CreateCenterType;

public static class CreateCenterTypeEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapPost("/api/center-types", Handle)
           .AddEndpointFilter<ValidationFilter<CreateCenterTypeCommand>>()
           .WithTags("Center Types")
           .WithName("CreateCenterType")
           .Produces<CreateCenterTypeResponse>(StatusCodes.Status201Created)
           .ProducesValidationProblem()
           .Produces(StatusCodes.Status401Unauthorized);
           //.RequireAuthorization();

    private static async Task<IResult> Handle(
        CreateCenterTypeCommand command,
        CreateCenterTypeCommandHandler handler,
        CancellationToken ct)
    {
        var response = await handler.HandleAsync(command, ct);
        return Results.Created($"/api/center-types/{response.Id}", response);
    }
}
