namespace MedAI.Contracts.Xrays;

public record XrayResultResponse(
    int Id,
    string ImageUrl,
    string? FinalDiagnosis,
    decimal? AI_Confidence, 
    string? DoctorNotes,
    DateTime ConfirmedAt
);