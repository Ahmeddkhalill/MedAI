namespace MedAI.Contracts.Schedules;

public record ScheduleResponse(
    int Id,
    DateOnly Date,
    TimeOnly StartTime,
    TimeOnly EndTime,
    decimal ConsultationFee,
    int Capacity,
    int BookedCount,
    int AvailableSlots
);
