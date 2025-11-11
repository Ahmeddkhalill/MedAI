using MedAI.Contracts.Xrays;

namespace MedAI.Services;

public interface IXrayService
{
    Task<Result<UploadResponse>> UploadXrayAsync(UploadRequest request, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<UnrevisedXrayResponse>>> GetUnrevisedXraysAsync(CancellationToken cancellationToken = default);
    Task<Result> ConfirmXrayAsync(int xrayId, ConfirmXrayRequest request, CancellationToken cancellationToken = default);

}
