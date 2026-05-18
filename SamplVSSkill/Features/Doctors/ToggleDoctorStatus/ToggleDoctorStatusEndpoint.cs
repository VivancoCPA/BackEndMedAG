namespace SamplVSSkill.Features.Doctors.ToggleDoctorStatus;

public static class ToggleDoctorStatusEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapPatch("/api/doctors/{id:guid}/toggle-status", Handle)
           .WithTags("Doctors")
           .WithName("ToggleDoctorStatus")
           .Produces<ToggleDoctorStatusResponse>()
           .Produces(StatusCodes.Status404NotFound)
           .Produces(StatusCodes.Status401Unauthorized);

    private static async Task<IResult> Handle(
        Guid id,
        ToggleDoctorStatusCommandHandler handler,
        CancellationToken ct)
    {
        var response = await handler.HandleAsync(id, ct);
        return response is not null ? Results.Ok(response) : Results.NotFound();
    }
}
