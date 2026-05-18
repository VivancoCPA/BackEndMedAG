using SamplVSSkill.Domain.Common;

namespace SamplVSSkill.Features.Doctors.PagedDoctors;

public static class PagedDoctorsEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/api/doctors/paged", Handle)
           .WithTags("Doctors")
           .WithName("PagedDoctors")
           .Produces<PaginatedResult<PagedDoctorItem>>()
           .Produces(StatusCodes.Status400BadRequest)
           .Produces(StatusCodes.Status401Unauthorized);

    private static async Task<IResult> Handle(
        [AsParameters] PagedDoctorsParams queryParams,
        PagedDoctorsQueryHandler handler,
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
