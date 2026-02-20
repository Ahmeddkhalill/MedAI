namespace MedAI.Errors;

public record AvailableTimeErrors
{
    public static readonly Error InvalidTimeRange =
        new("AvailableTime.InvalidTimeRange", "Start time must be before end time", StatusCodes.Status400BadRequest);

    public static readonly Error PastDate =
        new("AvailableTime.PastDate", "Date cannot be in the past", StatusCodes.Status400BadRequest);

    public static readonly Error InvalidCapacity =
        new("AvailableTime.InvalidCapacity", "Capacity must be greater than zero", StatusCodes.Status400BadRequest);

    public static readonly Error TimeOverlap =
        new("AvailableTime.TimeOverlap", "The specified time overlaps with an existing available time", StatusCodes.Status409Conflict);
        
    public static readonly Error NotFound =
        new("AvailableTime.NotFound", "Available time slot not found", StatusCodes.Status404NotFound);
}
