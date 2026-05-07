using FluentValidation;

namespace SamplVSSkill.Features.Doctors.UpdateDoctor;

public class UpdateDoctorValidator : AbstractValidator<UpdateDoctorCommand>
{
    public UpdateDoctorValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre del doctor es requerido.")
            .MaximumLength(200).WithMessage("El nombre no puede exceder 200 caracteres.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("El apellido del doctor es requerido.")
            .MaximumLength(200).WithMessage("El apellido no puede exceder 200 caracteres.");

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("El email no es válido.")
            .When(x => x.Email is not null);
    }
}
