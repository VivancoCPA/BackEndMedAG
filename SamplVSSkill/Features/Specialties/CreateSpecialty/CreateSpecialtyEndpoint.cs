using SamplVSSkill.Infrastructure.Middleware;

namespace SamplVSSkill.Features.Specialties.CreateSpecialty;

public static class CreateSpecialtyEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapPost("/api/specialties", Handle)
           .AddEndpointFilter<ValidationFilter<CreateSpecialtyCommand>>()
           .WithTags("Specialties")
           .WithName("CreateSpecialty")
           .Produces<CreateSpecialtyResponse>(StatusCodes.Status201Created)
           .ProducesValidationProblem()
           .Produces(StatusCodes.Status401Unauthorized);
           //.RequireAuthorization();

    private static async Task<IResult> Handle(
        CreateSpecialtyCommand command,
        CreateSpecialtyCommandHandler handler,
        CancellationToken ct)
    {
        var response = await handler.HandleAsync(command, ct);
        return Results.Created($"/api/specialties/{response.Id}", response);
    }
}
