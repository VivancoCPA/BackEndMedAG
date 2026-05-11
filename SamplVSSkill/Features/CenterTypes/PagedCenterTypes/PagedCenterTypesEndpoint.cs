using SamplVSSkill.Domain.Common;

namespace SamplVSSkill.Features.CenterTypes.PagedCenterTypes;

public static class PagedCenterTypesEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/api/center-types/paged", Handle)
           .WithTags("Center Types")
           .WithName("PagedCenterTypes")
           .Produces<PaginatedResult<PagedCenterTypeItem>>()
           .Produces(StatusCodes.Status400BadRequest)
           .Produces(StatusCodes.Status401Unauthorized);
           //.RequireAuthorization();

    private static async Task<IResult> Handle(
        [AsParameters] PagedCenterTypesParams queryParams,
        PagedCenterTypesQueryHandler handler,
        CancellationToken ct)
    {
        if (queryParams.Page < 1)
            return Results.BadRequest("El parámetro 'page' debe ser mayor o igual a 1.");

        if (queryParams.PageSize < 1 || queryParams.PageSize > 100)
            return Results.BadRequest("El parámetro 'pageSize' debe estar entre 1 y 100.");

        var result = await handler.HandleAsync(queryParams, ct);
        return Results.Ok(result);
    }
}
