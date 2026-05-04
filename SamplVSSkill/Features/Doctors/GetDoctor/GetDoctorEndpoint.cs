namespace SamplVSSkill.Features.Doctors.GetDoctor;

public static class GetDoctorEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/api/doctors/{id:guid}", Handle)
           .WithTags("Doctors")
           .WithName("GetDoctor")
           .Produces<GetDoctorResponse>()
           .Produces(StatusCodes.Status404NotFound)
           .Produces(StatusCodes.Status401Unauthorized)
           .RequireAuthorization();

    private static async Task<IResult> Handle(
        Guid id,
        GetDoctorQueryHandler handler,
        CancellationToken ct)
    {
        var response = await handler.HandleAsync(id, ct);
        return response is not null
            ? Results.Ok(response)
            : Results.NotFound();
    }
}
