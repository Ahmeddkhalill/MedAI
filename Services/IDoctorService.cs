using MedAI.Contracts.Doctors;

namespace MedAI.Services;

public interface IDoctorService
{
    Task<Result<IEnumerable<DoctorResponse>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<DoctorResponse>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Result<DoctorResponse>> AddDoctorAsync(AddDoctorRequest request, CancellationToken cancellationToken = default);
    Task<Result> UpdateDoctorAsync(int id, UpdateDoctorRequest request, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default);
}

