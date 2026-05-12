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
    public async Task<Result<ListResponse<PatientBookingResponse>>> GetMyBookingsAsync(
    RequestFilters filters,
    CancellationToken cancellationToken = default)
    {
        var userId = _httpContextAccessor.HttpContext?.User.GetUserId();

        if (userId is null)
            return Result.Failure<ListResponse<PatientBookingResponse>>(UserErrors.InvalidJwtToken);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var query = _context.Bookings
            .AsNoTracking()
            .Where(b => b.PatientId == userId);

        if (!string.IsNullOrWhiteSpace(filters.Type))
        {
            var type = filters.Type.ToLower();
            if (type == "past")
                query = query.Where(b => b.DoctorAvailableTime.Date < today);
            else if (type == "upcoming")
                query = query.Where(b => b.DoctorAvailableTime.Date >= today && !b.IsCancelled);
            else if (type == "cancelled")
                query = query.Where(b => b.IsCancelled);
        }

        var bookings = await query
            .OrderBy(b => b.DoctorAvailableTime.Date)
            .ThenBy(b => b.DoctorAvailableTime.StartTime)
            .Select(b => new
            {
                b.Id,
                b.CreatedAt,
                b.IsCancelled,
                Doctor = new DoctorInfo(
                    b.DoctorAvailableTime.Doctor.Id,
                    b.DoctorAvailableTime.Doctor.ApplicationUser.FirstName,
                    b.DoctorAvailableTime.Doctor.ApplicationUser.LastName,
                    b.DoctorAvailableTime.Doctor.Degree,
                    b.DoctorAvailableTime.Doctor.Speciality
                ),
                Slot = new SlotInfo(
                    b.DoctorAvailableTime.Id,
                    b.DoctorAvailableTime.Date,
                    b.DoctorAvailableTime.StartTime,
                    b.DoctorAvailableTime.EndTime,
                    b.DoctorAvailableTime.ConsultationFee
                ),
                b.DoctorAvailableTime.Date,
                b.DoctorAvailableTime.EndTime
            })
            .ToListAsync(cancellationToken);

        var mapped = bookings.Select(b => new PatientBookingResponse(
            b.Id,
            b.CreatedAt,
            GetBookingStatus(b.IsCancelled, b.Date, b.EndTime),
            b.Doctor,
            b.Slot
        )).ToList();

        return Result.Success(new ListResponse<PatientBookingResponse>(mapped.Count, mapped));
    }

    public async Task<Result> CancelAsync(int bookingId, CancellationToken cancellationToken = default)
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

        if (booking.IsCancelled)
            return Result.Failure(BookingErrors.AlreadyCancelled);

        booking.IsCancelled = true;
        booking.DoctorAvailableTime.BookedCount--;

        if (!booking.DoctorAvailableTime.IsActive)
            booking.DoctorAvailableTime.IsActive = true;

        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
    public async Task<Result<PaginatedList<DoctorAppointmentsByDateResponse>>> GetDoctorAppointmentsAsync(
    RequestFilters filters,
    CancellationToken cancellationToken = default)
    {
        var userId = _httpContextAccessor.HttpContext?.User.GetUserId();

        if (userId is null)
            return Result.Failure<PaginatedList<DoctorAppointmentsByDateResponse>>(UserErrors.InvalidJwtToken);

        var doctor = await _context.Doctors
            .FirstOrDefaultAsync(d => d.UserId == userId, cancellationToken);

        if (doctor is null)
            return Result.Failure<PaginatedList<DoctorAppointmentsByDateResponse>>(DoctorErrors.NotFound);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var query = _context.Bookings
            .AsNoTracking()
            .Include(b => b.Patient)
            .Include(b => b.DoctorAvailableTime)
            .Where(b => b.DoctorAvailableTime.DoctorId == doctor.Id);

        if (!string.IsNullOrWhiteSpace(filters.Type))
        {
            var type = filters.Type.ToLower();
            if (type == "past")
                query = query.Where(b => b.DoctorAvailableTime.Date < today);
            else if (type == "upcoming")
                query = query.Where(b => b.DoctorAvailableTime.Date >= today && !b.IsCancelled);
            else if (type == "cancelled")
                query = query.Where(b => b.IsCancelled);
        }

        var bookings = await query
            .OrderBy(b => b.DoctorAvailableTime.Date)
            .ThenBy(b => b.DoctorAvailableTime.StartTime)
            .Select(b => new
            {
                b.Id,
                b.CreatedAt,
                b.IsCancelled,
                b.PatientId,
                b.Patient.FirstName,
                b.Patient.LastName,
                Email = b.Patient.Email!,
                SlotId = b.DoctorAvailableTime.Id,
                b.DoctorAvailableTime.Date,
                b.DoctorAvailableTime.StartTime,
                b.DoctorAvailableTime.EndTime,
                b.DoctorAvailableTime.ConsultationFee
            })
            .ToListAsync(cancellationToken);

        var mapped = bookings.Select(b => new DoctorBookingResponse(
            b.Id,
            b.CreatedAt,
            GetBookingStatus(b.IsCancelled, b.Date, b.EndTime),
            b.PatientId,
            b.FirstName,
            b.LastName,
            b.Email,
            b.SlotId,
            b.Date,
            b.StartTime,
            b.EndTime,
            b.ConsultationFee
        )).ToList();

        var grouped = mapped
            .GroupBy(b => b.Date)
            .Select(g => new DoctorAppointmentsByDateResponse(g.Key, g.ToList()))
            .OrderBy(x => x.Date)
            .ToList();

        var paginatedItems = grouped
            .Skip((filters.PageNumber - 1) * filters.PageSize)
            .Take(filters.PageSize)
            .ToList();

        var paginated = new PaginatedList<DoctorAppointmentsByDateResponse>(
            paginatedItems, filters.PageNumber, grouped.Count, filters.PageSize, mapped.Count);

        return Result.Success(paginated);
    }
    private static string GetBookingStatus(bool isCancelled, DateOnly date, TimeOnly endTime)
    {
        if (isCancelled)
            return "Cancelled";

        var now = DateTime.UtcNow;
        var today = DateOnly.FromDateTime(now);
        var currentTime = TimeOnly.FromDateTime(now);

        if (date < today || (date == today && endTime < currentTime))
            return "Completed";

        return "Upcoming";
    }
}
