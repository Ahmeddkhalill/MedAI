namespace MedAI.Contracts.Xrays;

public record XrayResultResponse(
    int Id,
    string ImageUrl,
    string? FinalDiagnosis,
    string? DoctorName,
    string? DoctorSpeciality, 
    string? DoctorDegree,
    string? DoctorNotes,
    DateTime ConfirmedAt
);