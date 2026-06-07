using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SamplVSSkill.Features.FamilyGroups.ListFamilyGroups;

namespace SamplVSSkill.Features.FamilyGroups.GetMyFamilyGroup;

public static class GetMyFamilyGroupEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/api/family-groups/my", Handle)
           .WithTags("Family Groups")
           .WithName("GetMyFamilyGroup")
           .Produces<IEnumerable<ListFamilyGroupsResponse>>()
           .Produces(StatusCodes.Status401Unauthorized)
           .RequireAuthorization();

    private static async Task<IResult> Handle(
        ClaimsPrincipal principal,
        GetMyFamilyGroupQueryHandler handler,
        CancellationToken ct)
    {
        var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? principal.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Results.Unauthorized();
        }

        var items = await handler.HandleAsync(userId, ct);
        return Results.Ok(items);
    }
}
