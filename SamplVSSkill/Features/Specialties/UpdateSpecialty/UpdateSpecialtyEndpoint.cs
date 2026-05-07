using SamplVSSkill.Infrastructure.Middleware;

namespace SamplVSSkill.Features.Specialties.UpdateSpecialty;

public static class UpdateSpecialtyEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapPut("/api/specialties/{id:int}", Handle)
           .AddEndpointFilter<ValidationFilter<UpdateSpecialtyCommand>>()
           .WithTags("Specialties")
           .WithName("UpdateSpecialty")
           .Produces<UpdateSpecialtyResponse>()
           .Produces(StatusCodes.Status404NotFound)
           .ProducesValidationProblem()
           .Produces(StatusCodes.Status401Unauthorized);
           //.RequireAuthorization();

    private static async Task<IResult> Handle(
        int id,
        UpdateSpecialtyCommand command,
        UpdateSpecialtyCommandHandler handler,
        CancellationToken ct)
    {
        var response = await handler.HandleAsync(id, command, ct);
        return response is not null ? Results.Ok(response) : Results.NotFound();
    }
}
