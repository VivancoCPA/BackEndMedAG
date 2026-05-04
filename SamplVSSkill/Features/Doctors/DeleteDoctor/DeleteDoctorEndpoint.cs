namespace SamplVSSkill.Features.Doctors.DeleteDoctor;

public static class DeleteDoctorEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapDelete("/api/doctors/{id:guid}", Handle)
           .WithTags("Doctors")
           .WithName("DeleteDoctor")
           .Produces(StatusCodes.Status204NoContent)
           .Produces(StatusCodes.Status404NotFound)
           .Produces(StatusCodes.Status401Unauthorized)
           .RequireAuthorization();

    private static async Task<IResult> Handle(
        Guid id,
        DeleteDoctorCommandHandler handler,
        CancellationToken ct)
    {
        var deleted = await handler.HandleAsync(id, ct);
        return deleted
            ? Results.NoContent()
            : Results.NotFound();
    }
}
