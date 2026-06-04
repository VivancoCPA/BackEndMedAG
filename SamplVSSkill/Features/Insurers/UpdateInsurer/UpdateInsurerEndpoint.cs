using Microsoft.AspNetCore.Mvc;
using SamplVSSkill.Infrastructure.Middleware;

namespace SamplVSSkill.Features.Insurers.UpdateInsurer;

public static class UpdateInsurerEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapPut("/api/insurers/{id:guid}", Handle)
           .DisableAntiforgery()
           .AddEndpointFilter<ValidationFilter<UpdateInsurerCommand>>()
           .WithTags("Insurers")
           .WithName("UpdateInsurer")
           .Produces<UpdateInsurerResponse>()
           .Produces(StatusCodes.Status404NotFound)
           .ProducesValidationProblem()
           .Produces(StatusCodes.Status401Unauthorized);
           //.RequireAuthorization();

    private static async Task<IResult> Handle(
        Guid id,
        [FromForm] UpdateInsurerCommand command,
        UpdateInsurerCommandHandler handler,
        CancellationToken ct)
    {
        return await handler.HandleAsync(id, command, ct);
    }
}
