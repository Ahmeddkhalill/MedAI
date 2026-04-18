namespace MedAI.Contracts.Bookings;

public record DoctorBookingResponse(
    int Id,
    DateTime CreatedAt,

    string PatientId,
    string PatientFirstName,
    string PatientLastName,
    string PatientEmail,

    int SlotId,
    DateOnly Date,
    TimeOnly StartTime,
    TimeOnly EndTime,
    decimal ConsultationFee
);
