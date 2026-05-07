using SamplVSSkill.Infrastructure.Middleware;

namespace SamplVSSkill.Features.Insurers.CreateInsurer;

public static class CreateInsurerEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapPost("/api/insurers", Handle)
           .AddEndpointFilter<ValidationFilter<CreateInsurerCommand>>()
           .WithTags("Insurers")
           .WithName("CreateInsurer")
           .Produces<CreateInsurerResponse>(StatusCodes.Status201Created)
           .ProducesValidationProblem()
           .Produces(StatusCodes.Status401Unauthorized)
           .RequireAuthorization();

    private static async Task<IResult> Handle(
        CreateInsurerCommand command,
        CreateInsurerCommandHandler handler,
        CancellationToken ct)
    {
        var response = await handler.HandleAsync(command, ct);
        return Results.Created($"/api/insurers/{response.Id}", response);
    }
}
