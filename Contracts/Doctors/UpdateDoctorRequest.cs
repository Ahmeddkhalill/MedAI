namespace MedAI.Contracts.Doctors;

public record UpdateDoctorRequest(
    string FirstName,
    string LastName,
    string Email,
    Degree Degree
);
