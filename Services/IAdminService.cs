using MedAI.Contracts.Dashboard;

namespace MedAI.Services;

public interface IAdminService
{
    Task<Result<DashboardResponse>> GetDashboardAsync(CancellationToken cancellationToken = default);
}
