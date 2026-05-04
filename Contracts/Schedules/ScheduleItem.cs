namespace MedAI.Contracts.Schedules;

public record ScheduleItem(
    int Id,
    TimeOnly StartTime,
    TimeOnly EndTime,
    decimal ConsultationFee,
    int Capacity,
    int BookedCount,
    int AvailableSpots
);

public record ScheduleByDateResponse(
    DateOnly Date,
    IEnumerable<ScheduleItem> Slots
);

public record UpdateSlotCapacityRequest(int Capacity);