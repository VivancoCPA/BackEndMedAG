using FluentValidation;

namespace SamplVSSkill.Features.DoctorAffiliations.UpdateDoctorAffiliation;

public static class UpdateDoctorAffiliationEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapPut("/api/doctor-affiliations/{id:int}", Handle)
           .WithTags("Doctor Affiliations")
           .WithName("UpdateDoctorAffiliation")
           .Produces(StatusCodes.Status204NoContent)
           .Produces(StatusCodes.Status400BadRequest)
           .Produces(StatusCodes.Status404NotFound)
           .Produces(StatusCodes.Status401Unauthorized);

    private static async Task<IResult> Handle(
        int id,
        UpdateDoctorAffiliationCommand command,
        UpdateDoctorAffiliationCommandHandler handler,
        IValidator<UpdateDoctorAffiliationCommand> validator,
        CancellationToken ct)
    {
        command.Id = id;
        
        var validationResult = await validator.ValidateAsync(command, ct);
        if (!validationResult.IsValid)
            return Results.ValidationProblem(validationResult.ToDictionary());

        var success = await handler.HandleAsync(command, ct);
        return success ? Results.NoContent() : Results.NotFound();
    }
}
