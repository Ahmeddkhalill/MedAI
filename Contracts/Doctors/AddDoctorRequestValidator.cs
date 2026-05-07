using MedAI.Contracts.Doctors;

public class AddDoctorRequestValidator : AbstractValidator<AddDoctorRequest>
{
    public AddDoctorRequestValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().Length(3, 100);
        RuleFor(x => x.LastName).NotEmpty().Length(3, 100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().Matches(RegexPatterns.Password);

        RuleFor(x => x.Degree)
        .NotEmpty()
        .MaximumLength(100);

        RuleFor(x => x.Speciality)
            .NotEmpty()
            .MaximumLength(100);
    }
}