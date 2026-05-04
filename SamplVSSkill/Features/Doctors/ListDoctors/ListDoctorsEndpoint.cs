namespace SamplVSSkill.Features.Doctors.ListDoctors;

public static class ListDoctorsEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/api/doctors", Handle)
           .WithTags("Doctors")
           .WithName("ListDoctors")
           .Produces<IEnumerable<ListDoctorsResponse>>()
           .Produces(StatusCodes.Status401Unauthorized)
           .RequireAuthorization();

    private static async Task<IResult> Handle(
        ListDoctorsQueryHandler handler,
        CancellationToken ct)
    {
        var response = await handler.HandleAsync(ct);
        return Results.Ok(response);
    }
}
