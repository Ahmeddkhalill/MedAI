namespace MedAI.Contracts.Xrays;

public record DoctorXrayHistoryResponse(
    int Id,
    string ImageUrl,
    string PatientName,
    string? FinalDiagnosis,
    string? DoctorNotes,
    bool IsEdited,
    bool IsApproved,
    DateTime ConfirmedAt
);