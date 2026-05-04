using SamplVSSkill.Infrastructure.Middleware;

namespace SamplVSSkill.Features.MedicalCenters.CreateMedicalCenter;

public static class CreateMedicalCenterEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapPost("/api/medical-centers", Handle)
           .AddEndpointFilter<ValidationFilter<CreateMedicalCenterCommand>>()
           .WithTags("MedicalCenters")
           .WithName("CreateMedicalCenter")
           .Produces<CreateMedicalCenterResponse>(StatusCodes.Status201Created)
           .ProducesValidationProblem()
           .Produces(StatusCodes.Status401Unauthorized);
           //.RequireAuthorization();

    private static async Task<IResult> Handle(
        CreateMedicalCenterCommand command,
        CreateMedicalCenterCommandHandler handler,
        CancellationToken ct)
    {
        var response = await handler.HandleAsync(command, ct);
        return Results.Created($"/api/medical-centers/{response.Id}", response);
    }
}
