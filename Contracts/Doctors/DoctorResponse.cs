namespace MedAI.Contracts.Doctors;

public record DoctorResponse(
    int Id,
    string UserId,
    string FirstName,
    string LastName,
    string Email,
    string Speciality,
    string? ImageUrl,
    string Degree,
    string? Description,
    bool IsAccountCompleted,
    int CompletedAppointments,
    int HandledXrays           
);