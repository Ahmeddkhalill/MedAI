namespace MedAI.Contracts.Doctors;

public class UpdateDoctorRequestValidator : AbstractValidator<UpdateDoctorRequest>
{
    public UpdateDoctorRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.Degree)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Speciality)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Image)
            .Must(file => file == null || file.Length > 0)
            .WithMessage("Invalid image file.");
    }
}