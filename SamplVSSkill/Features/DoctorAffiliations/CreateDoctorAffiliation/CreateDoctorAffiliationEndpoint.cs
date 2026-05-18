using FluentValidation;

namespace SamplVSSkill.Features.DoctorAffiliations.CreateDoctorAffiliation;

public static class CreateDoctorAffiliationEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapPost("/api/doctor-affiliations", Handle)
           .WithTags("Doctor Affiliations")
           .WithName("CreateDoctorAffiliation")
           .Produces<int>(StatusCodes.Status201Created)
           .Produces(StatusCodes.Status400BadRequest)
           .Produces(StatusCodes.Status401Unauthorized);

    private static async Task<IResult> Handle(
        CreateDoctorAffiliationCommand command,
        CreateDoctorAffiliationCommandHandler handler,
        IValidator<CreateDoctorAffiliationCommand> validator,
        CancellationToken ct)
    {
        var validationResult = await validator.ValidateAsync(command, ct);
        if (!validationResult.IsValid)
            return Results.ValidationProblem(validationResult.ToDictionary());

        try
        {
            var id = await handler.HandleAsync(command, ct);
            return Results.Created($"/api/doctor-affiliations/{id}", id);
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }
}
