namespace SamplVSSkill.Features.UserInsurances.RemoveUserInsurance;

public static class RemoveUserInsuranceEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapDelete("/api/users/{userId}/insurances/{insurerId:guid}", Handle)
           .WithTags("User Insurances")
           .WithName("RemoveUserInsurance")
           .Produces<RemoveUserInsuranceResponse>()
           .Produces(StatusCodes.Status404NotFound);
           //.RequireAuthorization();

    private static async Task<IResult> Handle(
        string userId,
        Guid insurerId,
        RemoveUserInsuranceCommandHandler handler,
        CancellationToken ct)
        => await handler.HandleAsync(userId, insurerId, ct);
}
