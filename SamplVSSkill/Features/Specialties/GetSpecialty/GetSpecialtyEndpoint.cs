namespace SamplVSSkill.Features.Specialties.GetSpecialty;

public static class GetSpecialtyEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/api/specialties/{id:int}", Handle)
           .WithTags("Specialties")
           .WithName("GetSpecialty")
           .Produces<GetSpecialtyResponse>()
           .Produces(StatusCodes.Status404NotFound);

    private static async Task<IResult> Handle(
        int id,
        GetSpecialtyQueryHandler handler,
        CancellationToken ct)
    {
        var response = await handler.HandleAsync(id, ct);
        return response is not null ? Results.Ok(response) : Results.NotFound();
    }
}
