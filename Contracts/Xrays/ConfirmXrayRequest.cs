namespace MedAI.Contracts.Xrays;

public record ConfirmXrayRequest(
    string? FinalDiagnosis,
    string? DoctorNotes
);