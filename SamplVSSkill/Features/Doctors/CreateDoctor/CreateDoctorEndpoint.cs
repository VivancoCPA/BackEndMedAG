using SamplVSSkill.Infrastructure.Middleware;

namespace SamplVSSkill.Features.Doctors.CreateDoctor;

public static class CreateDoctorEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapPost("/api/doctors", Handle)
           .AddEndpointFilter<ValidationFilter<CreateDoctorCommand>>()// valida request antes de ejecutar handler
           .WithTags("Doctors")// define el nombre de la etiqueta para agrupar endpoints en la documentacion
           .WithName("CreateDoctor")// define el nombre del endpoint para generar url
           .Produces<CreateDoctorResponse>(StatusCodes.Status201Created)
           .ProducesValidationProblem()// si falla la validacion retorna problemas de validacion    
           .Produces(StatusCodes.Status401Unauthorized)// si falla la autenticacion retorna 401
           .RequireAuthorization();// requiere autorizacion

    private static async Task<IResult> Handle(
        CreateDoctorCommand command,
        CreateDoctorCommandHandler handler,
        CancellationToken ct)
    {
        var response = await handler.HandleAsync(command, ct);
        return Results.Created($"/api/doctors/{response.Id}", response);
    }
}
