namespace MedAI.Contracts.Xrays;

public class UploadRequestValidator : AbstractValidator<UploadRequest>
{
    public UploadRequestValidator()
    {
        RuleFor(x => x.Image)
            .NotNull().WithMessage("Image is required.");
    }
}
