namespace MedAI.Contracts.Bookings;

public record DoctorBookingResponse(
    int Id,
    DateTime CreatedAt,
    string Status, 
    string PatientId,
    string FirstName,
    string LastName,
    string Email,
    int SlotId,
    DateOnly Date,
    TimeOnly StartTime,
    TimeOnly EndTime,
    decimal ConsultationFee
);
