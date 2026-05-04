using SamplVSSkill.Infrastructure.Middleware;

namespace SamplVSSkill.Features.Doctors.CreateDoctor;

public static class CreateDoctorEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapPost("/api/doctors", Handle)
           .AddEndpointFilter<ValidationFilter<CreateDoctorCommand>>()
           .WithTags("Doctors")
           .WithName("CreateDoctor")
           .Produces<CreateDoctorResponse>(StatusCodes.Status201Created)
           .ProducesValidationProblem()
           .Produces(StatusCodes.Status401Unauthorized)
           .RequireAuthorization();

    private static async Task<IResult> Handle(
        CreateDoctorCommand command,
        CreateDoctorCommandHandler handler,
        CancellationToken ct)
    {
        var response = await handler.HandleAsync(command, ct);
        return Results.Created($"/api/doctors/{response.Id}", response);
    }
}
