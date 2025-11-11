namespace MedAI.Contracts.Xrays;

public class ConfirmXrayRequestValidator : AbstractValidator<ConfirmXrayRequest>
{
    public ConfirmXrayRequestValidator()
    {
        RuleFor(x => x.FinalDiagnosis)
            .NotEmpty().WithMessage("Final diagnosis is required.")
            .MaximumLength(50).WithMessage("Final diagnosis must not exceed 50 characters.");

        RuleFor(x => x.FinalConfidence)
            .InclusiveBetween(0, 100)
            .WithMessage("Final confidence must be between 0 and 100.");
    }
}
