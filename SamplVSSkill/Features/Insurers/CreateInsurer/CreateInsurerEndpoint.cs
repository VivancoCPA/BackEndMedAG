using Microsoft.AspNetCore.Mvc;
using SamplVSSkill.Infrastructure.Middleware;

namespace SamplVSSkill.Features.Insurers.CreateInsurer;

public static class CreateInsurerEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapPost("/api/insurers", Handle)
           .DisableAntiforgery()
           .AddEndpointFilter<ValidationFilter<CreateInsurerCommand>>()
           .WithTags("Insurers")
           .WithName("CreateInsurer")
           .Produces<CreateInsurerResponse>(StatusCodes.Status201Created)
           .ProducesValidationProblem()
           .Produces(StatusCodes.Status401Unauthorized);
           //.RequireAuthorization();

    private static async Task<IResult> Handle(
        [FromForm] CreateInsurerCommand command,
        CreateInsurerCommandHandler handler,
        CancellationToken ct)
    {
        return await handler.HandleAsync(command, ct);
    }
}
