using MedAI.Contracts.Bookings;

namespace MedAI.Services;

public interface IBookingService
{
    Task<Result<BookingResponse>> CreateAsync(int doctorAvailableTimeId, CancellationToken cancellationToken = default);
}