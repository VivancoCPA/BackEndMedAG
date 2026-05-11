using SamplVSSkill.Infrastructure.Middleware;

namespace SamplVSSkill.Features.CenterTypes.UpdateCenterType;

public static class UpdateCenterTypeEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapPut("/api/center-types/{id:int}", Handle)
           .AddEndpointFilter<ValidationFilter<UpdateCenterTypeCommand>>()
           .WithTags("Center Types")
           .WithName("UpdateCenterType")
           .Produces<UpdateCenterTypeResponse>()
           .Produces(StatusCodes.Status404NotFound)
           .ProducesValidationProblem()
           .Produces(StatusCodes.Status401Unauthorized);
           //.RequireAuthorization();

    private static async Task<IResult> Handle(
        int id,
        UpdateCenterTypeCommand command,
        UpdateCenterTypeCommandHandler handler,
        CancellationToken ct)
    {
        var response = await handler.HandleAsync(id, command, ct);
        return response is not null ? Results.Ok(response) : Results.NotFound();
    }
}
