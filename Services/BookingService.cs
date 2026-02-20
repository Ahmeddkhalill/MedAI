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

}