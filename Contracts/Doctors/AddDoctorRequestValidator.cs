namespace MedAI.Contracts.Doctors;

public class AddDoctorRequestValidator : AbstractValidator<AddDoctorRequest>
{ 
    public AddDoctorRequestValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().Length(3, 100);

        RuleFor(x => x.LastName)
            .NotEmpty().Length(3, 100);

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.Password)
            .NotEmpty()
            .Matches(RegexPatterns.Password)
            .WithMessage("Password must be at least 8 characters long and contain at least one uppercase letter, one lowercase letter, one digit, and one special character.");

        RuleFor(x => x.Degree)
            .IsInEnum().WithMessage("Degree must be a valid value.");
    }
}
