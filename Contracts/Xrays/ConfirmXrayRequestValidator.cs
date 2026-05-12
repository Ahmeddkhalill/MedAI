namespace MedAI.Contracts.Xrays;

public class ConfirmXrayRequestValidator : AbstractValidator<ConfirmXrayRequest>
{
    public ConfirmXrayRequestValidator()
    {
        RuleFor(x => x.FinalDiagnosis)
            .NotEmpty().WithMessage("Final diagnosis is required.")
            .MaximumLength(50).WithMessage("Final diagnosis must not exceed 50 characters.");
    }
}
