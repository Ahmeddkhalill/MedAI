namespace MedAI.Contracts.Xrays;

public record UploadResponse(
    int Id,
    string ImageUrl,
    string? AI_Diagnosis,
    decimal? AI_Confidence,
    string? FinalDiagnosis,   
    bool IsRevised,
    DateTime CreatedAt,
    DateTime? ConfirmedAt,
    string PatientId,
    int? DoctorId
);