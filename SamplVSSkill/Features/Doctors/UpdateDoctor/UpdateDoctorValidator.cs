using FluentValidation;

namespace SamplVSSkill.Features.Doctors.UpdateDoctor;

public class UpdateDoctorValidator : AbstractValidator<UpdateDoctorCommand>
{
    public UpdateDoctorValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre del doctor es requerido.")
            .MaximumLength(200).WithMessage("El nombre no puede exceder 200 caracteres.");

        RuleFor(x => x.Specialty)
            .MaximumLength(200).WithMessage("La especialidad no puede exceder 200 caracteres.")
            .When(x => x.Specialty is not null);
    }
}
