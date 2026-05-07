namespace MedAI.Contracts.Doctors;

public record UpdateDoctorRequest(
    string Email,
    string Speciality,
    string Degree
);