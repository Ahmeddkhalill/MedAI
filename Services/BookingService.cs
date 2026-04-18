using MedAI.Contracts.Bookings;

namespace MedAI.Services;

public class BookingService(
    ApplicationDbContext context,
    IHttpContextAccessor httpContextAccessor) : IBookingService
{
    private readonly ApplicationDbContext _context = context;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    public async Task<Result<BookingResponse>> CreateAsync(int doctorAvailableTimeId, CancellationToken cancellationToken = default)
    {
        var patientId = _httpContextAccessor.HttpContext?.User.GetUserId();

        if (patientId is null)
            return Result.Failure<BookingResponse>(UserErrors.InvalidJwtToken);

        var slot = await _context.DoctorAvailableTime
            .FirstOrDefaultAsync(x => x.Id == doctorAvailableTimeId && x.IsActive, cancellationToken);

        if (slot is null)
            return Result.Failure<BookingResponse>(AvailableTimeErrors.NotFound);

        var alreadyBooked = await _context.Bookings
            .AnyAsync(x => x.DoctorAvailableTimeId == doctorAvailableTimeId
                        && x.PatientId == patientId, cancellationToken);

        if (alreadyBooked)
            return Result.Failure<BookingResponse>(BookingErrors.AlreadyBooked);

        if (slot.BookedCount >= slot.Capacity)
            return Result.Failure<BookingResponse>(BookingErrors.FullyBooked);

        var booking = new Booking
        {
            DoctorAvailableTimeId = doctorAvailableTimeId,
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

    public async Task<Result<PaginatedList<PatientBookingResponse>>> GetMyBookingsAsync(RequestFilters filters,CancellationToken cancellationToken = default)
    {
        var userId = _httpContextAccessor.HttpContext?.User.GetUserId();

        if (userId is null)
            return Result.Failure<PaginatedList<PatientBookingResponse>>(UserErrors.InvalidJwtToken);

        var query = _context.Bookings
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
            ));

        var response = await PaginatedList<PatientBookingResponse>.CreateAsync(
            query,
            filters.PageNumber,
            filters.PageSize);

        return Result.Success(response);
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
}