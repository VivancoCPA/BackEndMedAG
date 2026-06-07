using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SamplVSSkill.Features.Relationships.ListRelationships;

public static class ListRelationshipsEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/api/relationships", Handle)
           .WithTags("Relationships")
           .WithName("ListRelationships")
           .Produces<IEnumerable<RelationshipResponse>>();

    private static async Task<IResult> Handle(
        ListRelationshipsQueryHandler handler,
        CancellationToken ct)
    {
        var items = await handler.HandleAsync(ct);
        return Results.Ok(items);
    }
}
