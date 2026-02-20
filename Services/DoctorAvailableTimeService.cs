using MedAI.Contracts.DoctorAvailableTime;

namespace MedAI.Services;

public class DoctorAvailableTimeService(ApplicationDbContext context,IHttpContextAccessor httpContextAccessor) : IDoctorAvailableTimeService
{
    private readonly ApplicationDbContext _context = context;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    public async Task<Result> AddAsync(AddDoctorAvailableTimeRequest request,CancellationToken cancellationToken)
    {
        var userId = _httpContextAccessor.HttpContext?.User.GetUserId();

        if (userId is null)
            return Result.Failure(UserErrors.InvalidJwtToken);

        var doctor = await _context.Doctors
            .FirstOrDefaultAsync(d => d.UserId == userId, cancellationToken);

        if (doctor is null)
            return Result.Failure(DoctorErrors.NotFound);

        // Overlap Protection
        var isOverlapping = await _context.DoctorAvailableTime
            .AnyAsync(x =>
                x.DoctorId == doctor.Id &&
                x.Date == request.Date &&
                x.IsActive &&
                request.StartTime < x.EndTime &&
                request.EndTime > x.StartTime,
                cancellationToken);

        if (isOverlapping)
            return Result.Failure(AvailableTimeErrors.TimeOverlap);

        var availableTime = new DoctorAvailableTime
        {
            DoctorId = doctor.Id,
            Date = request.Date,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            ConsultationFee = request.ConsultationFee,
            Capacity = request.Capacity
        };

        await _context.DoctorAvailableTime.AddAsync(availableTime);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result<IEnumerable<DoctorAvailableTimeResponse>>> GetByDoctorAsync(int doctorId, CancellationToken cancellationToken = default)
    {
        var exists = await _context.Doctors
            .AnyAsync(d => d.Id == doctorId, cancellationToken);

        if (!exists)
            return Result.Failure<IEnumerable<DoctorAvailableTimeResponse>>(DoctorErrors.NotFound);

        var times = await _context.DoctorAvailableTime
            .Where(x => x.DoctorId == doctorId && x.IsActive)
            .OrderBy(x => x.Date)
            .ThenBy(x => x.StartTime)
            .Select(x => new DoctorAvailableTimeResponse(
                x.Id,
                x.Date,
                x.StartTime,
                x.EndTime,
                x.ConsultationFee,
                x.Capacity,
                x.BookedCount,
                x.Capacity - x.BookedCount
            ))
            .ToListAsync(cancellationToken);

        return Result.Success<IEnumerable<DoctorAvailableTimeResponse>>(times);
    }
}
