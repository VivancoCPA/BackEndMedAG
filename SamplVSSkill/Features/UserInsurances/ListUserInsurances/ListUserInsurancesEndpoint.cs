namespace SamplVSSkill.Features.UserInsurances.ListUserInsurances;

public static class ListUserInsurancesEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/api/users/{userId}/insurances", Handle)
           .WithTags("User Insurances")
           .WithName("ListUserInsurances")
           .Produces<IEnumerable<UserInsuranceItem>>()
           .Produces(StatusCodes.Status404NotFound);
           //.RequireAuthorization();

    private static async Task<IResult> Handle(
        string userId,
        ListUserInsurancesQueryHandler handler,
        CancellationToken ct)
        => await handler.HandleAsync(userId, ct);
}
