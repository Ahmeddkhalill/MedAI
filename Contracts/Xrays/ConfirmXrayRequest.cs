namespace MedAI.Contracts.Xrays;

public record ConfirmXrayRequest(
    string FinalDiagnosis,
    decimal FinalConfidence,
    string? DoctorNotes
);