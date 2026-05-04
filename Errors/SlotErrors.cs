namespace MedAI.Errors;

public record SlotErrors
{
    public static readonly Error InvalidCapacity =
        new("Slot.InvalidCapacity", "Capacity must be greater than zero", StatusCodes.Status400BadRequest);

    public static readonly Error BadCapacity = new("Slot.BadCapacity","Capacity cannot be less than booked count",StatusCodes.Status400BadRequest);
}
