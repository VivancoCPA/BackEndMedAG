using Microsoft.AspNetCore.Mvc;
using SamplVSSkill.Infrastructure.Middleware;

namespace SamplVSSkill.Features.Doctors.UpdateDoctor;

public static class UpdateDoctorEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapPut("/api/doctors/{id:guid}", Handle)
           .DisableAntiforgery()
           .AddEndpointFilter<ValidationFilter<UpdateDoctorCommand>>()
           .WithTags("Doctors")
           .WithName("UpdateDoctor")
           .Produces<UpdateDoctorResponse>()
           .Produces(StatusCodes.Status404NotFound)
           .ProducesValidationProblem()
           .Produces(StatusCodes.Status401Unauthorized);
           //.RequireAuthorization();

    private static async Task<IResult> Handle(
        Guid id,
        [FromForm] UpdateDoctorCommand command,
        UpdateDoctorCommandHandler handler,
        CancellationToken ct)
    {
        return await handler.HandleAsync(id, command, ct);
    }
}
