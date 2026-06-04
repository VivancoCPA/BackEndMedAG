using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Threading;
using System.Threading.Tasks;

namespace SamplVSSkill.Features.Auth.ToggleRoleStatus;

public static class ToggleRoleStatusEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapPatch("/api/roles/{id}/toggle-status", Handle)
           .WithTags("Roles")
           .WithName("ToggleRoleStatus")
           .Produces<ToggleRoleStatusResponse>(StatusCodes.Status200OK)
           .Produces(StatusCodes.Status404NotFound)
           .ProducesValidationProblem()
           .Produces(StatusCodes.Status401Unauthorized)
           .RequireAuthorization();

    private static async Task<IResult> Handle(
        string id,
        ToggleRoleStatusCommandHandler handler,
        CancellationToken ct) =>
        await handler.HandleAsync(id, ct);
}
