namespace MedAI.Contracts.Bookings;

public record PatientBookingResponse(
    int Id,
    DateTime CreatedAt,
    string Status, 
    DoctorInfo Doctor,
    SlotInfo Slot
);

public record DoctorInfo(
    int Id,
    string FirstName,
    string LastName,
    string Degree,
    string Speciality
);

public record SlotInfo(
    int Id,
    DateOnly Date,
    TimeOnly StartTime,
    TimeOnly EndTime,
    decimal ConsultationFee
);