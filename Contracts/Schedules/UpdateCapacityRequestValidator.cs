namespace MedAI.Contracts.Schedules;

public class UpdateCapacityRequestValidator : AbstractValidator<UpdateCapacityRequest>
{
    public UpdateCapacityRequestValidator()
    {
        RuleFor(x => x.NewCapacity)
            .GreaterThan(0)
            .WithMessage("New capacity must be greater than zero.");
    }
}
