using MedAI.Contracts.Schedules;

namespace MedAI.Services;

public interface IScheduleService
{
    Task<Result> AddAsync(AddScheduleRequest request, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<ScheduleResponse>>> GetByDoctorAsync(int doctorId, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<ScheduleByDateResponse>>> GetMySchedulesAsync(CancellationToken cancellationToken = default);
    Task<Result> DeleteScheduleAsync(int id, CancellationToken cancellationToken = default);
    Task<Result> UpdateCapacityAsync(int id, int newCapacity, CancellationToken cancellationToken = default);
}