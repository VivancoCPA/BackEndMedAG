using SamplVSSkill.Infrastructure.Middleware;

namespace SamplVSSkill.Features.MedicalCenters.UpdateMedicalCenter;

public static class UpdateMedicalCenterEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapPut("/api/medical-centers/{id:guid}", Handle)
           .AddEndpointFilter<ValidationFilter<UpdateMedicalCenterCommand>>()
           .WithTags("MedicalCenters")
           .WithName("UpdateMedicalCenter")
           .Produces<UpdateMedicalCenterResponse>()
           .Produces(StatusCodes.Status404NotFound)
           .ProducesValidationProblem()
           .Produces(StatusCodes.Status401Unauthorized);
           //.RequireAuthorization();

    private static async Task<IResult> Handle(
        Guid id,
        UpdateMedicalCenterCommand command,
        UpdateMedicalCenterCommandHandler handler,
        CancellationToken ct)
    {
        var response = await handler.HandleAsync(id, command, ct);
        return response is not null
            ? Results.Ok(response)
            : Results.NotFound();
    }
}
