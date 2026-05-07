namespace MedAI.Contracts.Doctors;

public record CompleteProfileRequest(
    string FirstName,
    string LastName,
    string Description,
    IFormFile? Image
);