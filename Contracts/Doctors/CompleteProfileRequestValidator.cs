namespace MedAI.Contracts.Doctors;

public class CompleteProfileRequestValidator : AbstractValidator<CompleteProfileRequest>
{
    public CompleteProfileRequestValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .Length(3,100).WithMessage("First name must be between 3 and 100 characters.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .Length(3, 100).WithMessage("Last name must be between 3 and 100 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters.");
        
        RuleFor(x => x.Image)
            .Must(file => file == null || file.Length <= 5 * 1024 * 1024) 
            .WithMessage("Image size cannot exceed 5 MB.")
            .Must(file => file == null || file.ContentType.StartsWith("image/"))
            .WithMessage("Only image files are allowed.");
    }
}
