using SamplVSSkill.Infrastructure.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Threading;
using System.Threading.Tasks;

namespace SamplVSSkill.Features.Auth.UpdateRole;

public static class UpdateRoleEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapPut("/api/roles/{id}", Handle)
           .AddEndpointFilter<ValidationFilter<UpdateRoleCommand>>()
           .WithTags("Roles")
           .WithName("UpdateRole")
           .Produces<UpdateRoleResponse>(StatusCodes.Status200OK)
           .Produces(StatusCodes.Status404NotFound)
           .Produces(StatusCodes.Status409Conflict)
           .ProducesValidationProblem()
           .Produces(StatusCodes.Status401Unauthorized)
           .RequireAuthorization();

    private static async Task<IResult> Handle(
        string id,
        UpdateRoleCommand command,
        UpdateRoleCommandHandler handler,
        CancellationToken ct) =>
        await handler.HandleAsync(id, command, ct);
}
