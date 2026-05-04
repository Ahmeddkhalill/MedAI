using MedAI.Contracts.Schedules;

namespace MedAI.Services;

public class ScheduleService(ApplicationDbContext context,IHttpContextAccessor httpContextAccessor) : IScheduleService
{
    private readonly ApplicationDbContext _context = context;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    public async Task<Result> AddAsync(AddScheduleRequest request,CancellationToken cancellationToken)
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

    public async Task<Result<IEnumerable<ScheduleResponse>>> GetByDoctorAsync(int doctorId, CancellationToken cancellationToken = default)
    {
        var exists = await _context.Doctors
            .AnyAsync(d => d.Id == doctorId, cancellationToken);

        if (!exists)
            return Result.Failure<IEnumerable<ScheduleResponse>>(DoctorErrors.NotFound);

        var times = await _context.DoctorAvailableTime
            .Where(x => x.DoctorId == doctorId && x.IsActive)
            .OrderBy(x => x.Date)
            .ThenBy(x => x.StartTime)
            .Select(x => new ScheduleResponse(
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

        return Result.Success<IEnumerable<ScheduleResponse>>(times);
    }

    public async Task<Result<IEnumerable<ScheduleByDateResponse>>> GetMySchedulesAsync(CancellationToken cancellationToken = default)
    {
        var userId = _httpContextAccessor.HttpContext?.User.GetUserId();

        if (userId is null)
            return Result.Failure<IEnumerable<ScheduleByDateResponse>>(UserErrors.InvalidJwtToken);

        var doctor = await _context.Doctors
            .FirstOrDefaultAsync(d => d.UserId == userId, cancellationToken);

        if (doctor is null)
            return Result.Failure<IEnumerable<ScheduleByDateResponse>>(DoctorErrors.NotFound);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var slots = await _context.DoctorAvailableTime
            .AsNoTracking()
            .Include(x => x.Bookings)
            .Where(x => x.DoctorId == doctor.Id && x.Date >= today)
            .OrderBy(x => x.Date)
            .ThenBy(x => x.StartTime)
            .ToListAsync(cancellationToken);

        var result = slots
            .GroupBy(x => x.Date)
            .Select(g => new ScheduleByDateResponse(
                g.Key,
                g.Select(x => new ScheduleItem(
                    x.Id,
                    x.StartTime,
                    x.EndTime,
                    x.ConsultationFee,
                    x.Capacity,
                    x.Bookings.Count,
                    x.Capacity - x.Bookings.Count
                ))
            ));

        return Result.Success(result);
    }

    public async Task<Result> DeleteScheduleAsync(int id, CancellationToken cancellationToken = default)
    {
        var userId = _httpContextAccessor.HttpContext?.User.GetUserId();

        if (userId is null)
            return Result.Failure(UserErrors.InvalidJwtToken);

        var slot = await _context.DoctorAvailableTime
            .Include(x => x.Bookings)
            .Include(x => x.Doctor)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (slot is null)
            return Result.Failure(DoctorErrors.NotFound);

        if (slot.Doctor.UserId != userId)
            return Result.Failure(BookingErrors.Unauthorized);

        if (slot.Bookings.Any())
            return Result.Failure(BookingErrors.HasBookings);

        _context.DoctorAvailableTime.Remove(slot);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> UpdateCapacityAsync(int id, int newCapacity, CancellationToken cancellationToken = default)
    {
        var userId = _httpContextAccessor.HttpContext?.User.GetUserId();

        if (userId is null)
            return Result.Failure(UserErrors.InvalidJwtToken);

        if (newCapacity <= 0)
            return Result.Failure(SlotErrors.InvalidCapacity);
        
        var slot = await _context.DoctorAvailableTime
            .Include(x => x.Bookings)
            .Include(x => x.Doctor)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (slot is null)
            return Result.Failure(BookingErrors.NotFound);

        if (slot.Doctor.UserId != userId)
            return Result.Failure(BookingErrors.Unauthorized);

        var bookedCount = slot.Bookings.Count;

        if (newCapacity < bookedCount)
            return Result.Failure(SlotErrors.BadCapacity);
        
        slot.Capacity = newCapacity;

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
