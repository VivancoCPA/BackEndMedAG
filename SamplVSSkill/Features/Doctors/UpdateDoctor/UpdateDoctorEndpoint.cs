using SamplVSSkill.Infrastructure.Middleware;

namespace SamplVSSkill.Features.Doctors.UpdateDoctor;

public static class UpdateDoctorEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapPut("/api/doctors/{id:guid}", Handle)
           .AddEndpointFilter<ValidationFilter<UpdateDoctorCommand>>()
           .WithTags("Doctors")
           .WithName("UpdateDoctor")
           .Produces<UpdateDoctorResponse>()
           .Produces(StatusCodes.Status404NotFound)
           .ProducesValidationProblem()
           .Produces(StatusCodes.Status401Unauthorized)
           .RequireAuthorization();

    private static async Task<IResult> Handle(
        Guid id,
        UpdateDoctorCommand command,
        UpdateDoctorCommandHandler handler,
        CancellationToken ct)
    {
        var response = await handler.HandleAsync(id, command, ct);
        return response is not null
            ? Results.Ok(response)
            : Results.NotFound();
    }
}
