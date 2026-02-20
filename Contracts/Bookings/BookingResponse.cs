namespace MedAI.Contracts.Bookings;

public record BookingResponse(
    int Id,
    int DoctorAvailableTimeId,
    string PatientId,
    DateTime CreatedAt
);