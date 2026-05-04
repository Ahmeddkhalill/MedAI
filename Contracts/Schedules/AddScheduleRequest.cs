namespace MedAI.Contracts.Schedules;

public record AddScheduleRequest(
    DateOnly Date,
    TimeOnly StartTime,
    TimeOnly EndTime,
    decimal ConsultationFee,
    int Capacity
);
