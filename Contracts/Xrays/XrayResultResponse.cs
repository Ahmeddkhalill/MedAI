namespace MedAI.Contracts.Xrays;

public record XrayResultResponse(
    int Id,
    string ImageUrl,
    string? FinalDiagnosis,
    decimal? FinalConfidence,
    string? DoctorNotes,
    DateTime ConfirmedAt
);
