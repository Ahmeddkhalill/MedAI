namespace MedAI.Contracts.DoctorAvailableTime;

public class AddDoctorAvailableTimeRequestValidator : AbstractValidator<AddDoctorAvailableTimeRequest>
{
    public AddDoctorAvailableTimeRequestValidator()
    {
        RuleFor(x => x.Date)
            .GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("Date cannot be in the past.");

        RuleFor(x => x.StartTime)
            .NotEmpty()
            .WithMessage("Start time is required.");

        RuleFor(x => x.EndTime)
            .NotEmpty()
            .WithMessage("End time is required.")
            .GreaterThan(x => x.StartTime)
            .WithMessage("End time must be greater than start time.");

        RuleFor(x => x.ConsultationFee)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Consultation fee must be a non-negative value.");

        RuleFor(x => x.Capacity)
            .GreaterThan(0)
            .WithMessage("Capacity must be greater than zero.");
    }
}
