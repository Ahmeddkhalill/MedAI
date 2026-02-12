using MedAI.Contracts.Xrays;

namespace MedAI.Services;

public interface IXrayService
{
    Task<Result<UploadResponse>> UploadXrayAsync(UploadRequest request, CancellationToken cancellationToken = default);
    Task<Result<PaginatedList<UnrevisedXrayResponse>>> GetUnrevisedXraysAsync(RequestFilters filters, CancellationToken cancellationToken = default);
    Task<Result> ConfirmXrayAsync(int xrayId, ConfirmXrayRequest request, CancellationToken cancellationToken = default);
    Task<Result<XrayResultResponse>> GetConfirmedXrayByIdAsync(int xrayId, CancellationToken cancellationToken = default);
    Task<Result<PaginatedList<PatientXrayHistoryResponse>>> GetMyHistoryAsync(RequestFilters filters,CancellationToken cancellationToken = default);
}
