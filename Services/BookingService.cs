using MedAI.Contracts.Bookings;
using MedAI.Contracts.Doctors;

namespace MedAI.Services;

public class BookingService(
    ApplicationDbContext context,
    IHttpContextAccessor httpContextAccessor) : IBookingService
{
    private readonly ApplicationDbContext _context = context;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    public async Task<Result<BookingResponse>> CreateAsync(int scheduleId, CancellationToken cancellationToken = default)
    {
        var patientId = _httpContextAccessor.HttpContext?.User.GetUserId();

        if (patientId is null)
            return Result.Failure<BookingResponse>(UserErrors.InvalidJwtToken);

        var slot = await _context.DoctorAvailableTime
            .FirstOrDefaultAsync(x => x.Id == scheduleId && x.IsActive, cancellationToken);

        if (slot is null)
            return Result.Failure<BookingResponse>(AvailableTimeErrors.NotFound);

        var alreadyBooked = await _context.Bookings
            .AnyAsync(x => x.DoctorAvailableTimeId == scheduleId
                        && x.PatientId == patientId, cancellationToken);

        if (alreadyBooked)
            return Result.Failure<BookingResponse>(BookingErrors.AlreadyBooked);

        if (slot.BookedCount >= slot.Capacity)
            return Result.Failure<BookingResponse>(BookingErrors.FullyBooked);

        var booking = new Booking
        {
            DoctorAvailableTimeId = scheduleId,
            PatientId = patientId,
            CreatedAt = DateTime.UtcNow
        };

        await _context.Bookings.AddAsync(booking, cancellationToken);

        slot.BookedCount++;

        if (slot.BookedCount >= slot.Capacity)
            slot.IsActive = false;

        await _context.SaveChangesAsync(cancellationToken);

        var response = new BookingResponse(
            booking.Id,
            booking.DoctorAvailableTimeId,
            booking.PatientId,
            booking.CreatedAt
        );

        return Result.Success(response);
    }

    public async Task<Result<List<PatientBookingResponse>>> GetMyBookingsAsync(CancellationToken cancellationToken = default)
    {
        var userId = _httpContextAccessor.HttpContext?.User.GetUserId();

        if (userId is null)
            return Result.Failure<List<PatientBookingResponse>>(UserErrors.InvalidJwtToken);

        var bookings = await _context.Bookings
            .AsNoTracking()
            .Where(b => b.PatientId == userId)
            .OrderBy(b => b.DoctorAvailableTime.Date)
            .ThenBy(b => b.DoctorAvailableTime.StartTime)
            .Select(b => new PatientBookingResponse(
                b.Id,
                b.CreatedAt,
                new DoctorInfo(
                    b.DoctorAvailableTime.Doctor.Id,
                    b.DoctorAvailableTime.Doctor.ApplicationUser.FirstName,
                    b.DoctorAvailableTime.Doctor.ApplicationUser.LastName,
                    b.DoctorAvailableTime.Doctor.Degree.ToString()
                ),
                new SlotInfo(
                    b.DoctorAvailableTime.Id,
                    b.DoctorAvailableTime.Date,
                    b.DoctorAvailableTime.StartTime,
                    b.DoctorAvailableTime.EndTime,
                    b.DoctorAvailableTime.ConsultationFee
                ),
                b.DoctorAvailableTime.Date >= DateOnly.FromDateTime(DateTime.UtcNow)
            ))
            .ToListAsync(cancellationToken);

        return Result.Success(bookings);
    }

    public async Task<Result> DeleteAsync(int bookingId, CancellationToken cancellationToken = default)
    {
        var userId = _httpContextAccessor.HttpContext?.User.GetUserId();

        if (userId is null)
            return Result.Failure(UserErrors.InvalidJwtToken);

        var booking = await _context.Bookings
            .Include(b => b.DoctorAvailableTime)
            .FirstOrDefaultAsync(b => b.Id == bookingId, cancellationToken);

        if (booking is null)
            return Result.Failure(BookingErrors.NotFound);

        if (booking.PatientId != userId)
            return Result.Failure(BookingErrors.Unauthorized);

        booking.DoctorAvailableTime.Capacity++;

        _context.Bookings.Remove(booking);

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result<PaginatedList<DoctorAppointmentsByDateResponse>>> GetDoctorAppointmentsAsync(RequestFilters filters,CancellationToken cancellationToken = default)
    {
        var userId = _httpContextAccessor.HttpContext?.User.GetUserId();

        if (userId is null)
            return Result.Failure<PaginatedList<DoctorAppointmentsByDateResponse>>(UserErrors.InvalidJwtToken);

        var doctor = await _context.Doctors
            .FirstOrDefaultAsync(d => d.UserId == userId, cancellationToken);

        if (doctor is null)
            return Result.Failure<PaginatedList<DoctorAppointmentsByDateResponse>>(DoctorErrors.NotFound);

        var bookings = await _context.Bookings
            .AsNoTracking()
            .Include(b => b.Patient)
            .Include(b => b.DoctorAvailableTime)
            .Where(b => b.DoctorAvailableTime.DoctorId == doctor.Id)
            .OrderBy(b => b.DoctorAvailableTime.Date)
            .ThenBy(b => b.DoctorAvailableTime.StartTime)
            .Select(b => new DoctorBookingResponse(
                b.Id,
                b.CreatedAt,
                b.PatientId,
                b.Patient.FirstName,
                b.Patient.LastName,
                b.Patient.Email!,
                b.DoctorAvailableTime.Id,
                b.DoctorAvailableTime.Date,
                b.DoctorAvailableTime.StartTime,
                b.DoctorAvailableTime.EndTime,
                b.DoctorAvailableTime.ConsultationFee
            ))
            .ToListAsync(cancellationToken);

        var grouped = bookings
            .GroupBy(b => b.Date)
            .Select(g => new DoctorAppointmentsByDateResponse(
                g.Key,
                g.ToList()
            ))
            .OrderBy(x => x.Date)
            .ToList();

        var totalCount = grouped.Count;

        var items = grouped
            .Skip((filters.PageNumber - 1) * filters.PageSize)
            .Take(filters.PageSize)
            .ToList();

        var paginated = new PaginatedList<DoctorAppointmentsByDateResponse>(
            items,
            totalCount,
            filters.PageNumber,
            filters.PageSize
        );

        return Result.Success(paginated);
    }
}
