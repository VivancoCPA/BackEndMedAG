using SamplVSSkill.Domain.Common;

namespace SamplVSSkill.Features.Specialties.PagedSpecialties;

public static class PagedSpecialtiesEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/api/specialties/paged", Handle)
           .WithTags("Specialties")
           .WithName("PagedSpecialties")
           .Produces<PaginatedResult<PagedSpecialtyItem>>()
           .Produces(StatusCodes.Status400BadRequest);

    private static async Task<IResult> Handle(
        [AsParameters] PagedSpecialtiesParams queryParams,
        PagedSpecialtiesQueryHandler handler,
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
