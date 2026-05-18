namespace SamplVSSkill.Features.Doctors.SummaryDoctors;

public static class SummaryDoctorsEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/api/doctors/summary", Handle)
           .WithTags("Doctors")
           .WithName("SummaryDoctors")
           .Produces<DoctorSummaryResponse>()
           .Produces(StatusCodes.Status401Unauthorized);

    private static async Task<IResult> Handle(
        SummaryDoctorsQueryHandler handler,
        CancellationToken ct)
    {
        var summary = await handler.HandleAsync(ct);
        return Results.Ok(summary);
    }
}
