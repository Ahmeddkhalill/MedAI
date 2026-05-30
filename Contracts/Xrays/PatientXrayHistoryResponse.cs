namespace MedAI.Contracts.Xrays;

public record PatientXrayHistoryResponse(
    int Id,
    string ImageUrl,
    string? FinalDiagnosis,
    string? DoctorName,
    string? DoctorNotes,
    bool IsRevised,
    DateTime CreatedAt,
    DateTime? ConfirmedAt
);
