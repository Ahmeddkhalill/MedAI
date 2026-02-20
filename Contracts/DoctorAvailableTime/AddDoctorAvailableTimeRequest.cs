namespace MedAI.Contracts.DoctorAvailableTime;

public record AddDoctorAvailableTimeRequest(
    DateOnly Date,
    TimeOnly StartTime,
    TimeOnly EndTime,
    decimal ConsultationFee,
    int Capacity
);
