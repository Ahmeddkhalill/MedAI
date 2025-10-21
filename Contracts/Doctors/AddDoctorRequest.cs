namespace MedAI.Contracts.Doctors;

public record AddDoctorRequest
(
     string FirstName,
     string LastName,
     string Email,
     string Password,
     Degree Degree
);
