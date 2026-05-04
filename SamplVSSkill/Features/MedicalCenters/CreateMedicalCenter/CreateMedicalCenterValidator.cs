using FluentValidation;

namespace SamplVSSkill.Features.MedicalCenters.CreateMedicalCenter;

public class CreateMedicalCenterValidator : AbstractValidator<CreateMedicalCenterCommand>
{
    public CreateMedicalCenterValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre del centro médico es requerido.")
            .MaximumLength(200).WithMessage("El nombre no puede exceder 200 caracteres.");

        // Type is an enum — invalid values are rejected by model binding before validation
        // Optional: validate it is not null if required
        RuleFor(x => x.Type).NotNull().WithMessage("El tipo es requerido.");

        RuleFor(x => x.Address)
            .MaximumLength(500).WithMessage("La dirección no puede exceder 500 caracteres.")
            .When(x => x.Address is not null);

        RuleFor(x => x.Phone)
            .MaximumLength(30).WithMessage("El teléfono no puede exceder 30 caracteres.")
            .Matches(@"^[\d\s\+\-\(\)]+$").WithMessage("El teléfono solo puede contener dígitos, espacios y +, -, (, ).")
            .When(x => x.Phone is not null);

        RuleFor(x => x.Latitude)
            .InclusiveBetween(-90, 90).WithMessage("La latitud debe estar entre -90 y 90.")
            .When(x => x.Latitude.HasValue);

        RuleFor(x => x.Longitude)
            .InclusiveBetween(-180, 180).WithMessage("La longitud debe estar entre -180 y 180.")
            .When(x => x.Longitude.HasValue);
    }
}

