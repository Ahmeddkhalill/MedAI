using MedAI.Contracts.DoctorAvailableTime;

namespace MedAI.Services;

public interface IDoctorAvailableTimeService
{
    Task<Result> AddAsync(AddDoctorAvailableTimeRequest request, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<DoctorAvailableTimeResponse>>> GetByDoctorAsync(int doctorId, CancellationToken cancellationToken = default);
}