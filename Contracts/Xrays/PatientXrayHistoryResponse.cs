namespace MedAI.Contracts.Xrays;

public record PatientXrayHistoryResponse(
    int Id,
    string ImageUrl,
    string? AI_Diagnosis,
    decimal? AI_Confidence,
    string? FinalDiagnosis,
    decimal? FinalConfidence,
    string? DoctorNotes,
    bool IsRevised,
    DateTime CreatedAt,
    DateTime? ConfirmedAt
);
