namespace MedAI.Contracts.Xrays;

public record UploadRequest(
    IFormFile Image
);