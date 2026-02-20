namespace MedAI.Contracts.DoctorAvailableTime;

public record DoctorAvailableTimeResponse(
    int Id,
    DateOnly Date,
    TimeOnly StartTime,
    TimeOnly EndTime,
    decimal ConsultationFee,
    int Capacity,
    int BookedCount,
    int AvailableSlots
);
