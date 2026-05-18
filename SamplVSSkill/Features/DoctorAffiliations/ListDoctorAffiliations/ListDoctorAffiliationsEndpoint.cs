namespace SamplVSSkill.Features.DoctorAffiliations.ListDoctorAffiliations;

public static class ListDoctorAffiliationsEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/api/doctor-affiliations", Handle)
           .WithTags("Doctor Affiliations")
           .WithName("ListDoctorAffiliations")
           .Produces<IEnumerable<DoctorAffiliationItem>>()
           .Produces(StatusCodes.Status401Unauthorized);

    private static async Task<IResult> Handle(
        [AsParameters] ListDoctorAffiliationsParams queryParams,
        ListDoctorAffiliationsQueryHandler handler,
        CancellationToken ct)
    {
        var items = await handler.HandleAsync(queryParams, ct);
        return Results.Ok(items);
    }
}
