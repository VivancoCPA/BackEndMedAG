namespace SamplVSSkill.Features.DoctorAffiliations.DeleteDoctorAffiliation;

public static class DeleteDoctorAffiliationEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapDelete("/api/doctor-affiliations/{id:int}", Handle)
           .WithTags("Doctor Affiliations")
           .WithName("DeleteDoctorAffiliation")
           .Produces(StatusCodes.Status204NoContent)
           .Produces(StatusCodes.Status404NotFound)
           .Produces(StatusCodes.Status401Unauthorized);

    private static async Task<IResult> Handle(
        int id,
        DeleteDoctorAffiliationCommandHandler handler,
        CancellationToken ct)
    {
        var success = await handler.HandleAsync(new DeleteDoctorAffiliationCommand(id), ct);
        return success ? Results.NoContent() : Results.NotFound();
    }
}
