namespace SamplVSSkill.Features.UserInsurances.AssignUserInsurance;

public static class AssignUserInsuranceEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapPost("/api/users/{userId}/insurances", Handle)
           .WithTags("User Insurances")
           .WithName("AssignUserInsurance")
           .Produces<AssignUserInsuranceResponse>(StatusCodes.Status201Created)
           .ProducesValidationProblem()
           .Produces(StatusCodes.Status404NotFound)
           .Produces(StatusCodes.Status409Conflict);
           //.RequireAuthorization();

    private static async Task<IResult> Handle(
        string userId,
        AssignUserInsuranceCommand command,
        AssignUserInsuranceCommandHandler handler,
        CancellationToken ct)
        => await handler.HandleAsync(userId, command, ct);
}
