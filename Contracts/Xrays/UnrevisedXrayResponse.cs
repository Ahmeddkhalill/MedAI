namespace MedAI.Contracts.Xrays;

public record UnrevisedXrayResponse(
    int Id,
    string ImageUrl,
    string? AI_Diagnosis,
    decimal? AI_Confidence,
    string PatientId,
    string PatientName,   
    DateTime CreatedAt
);