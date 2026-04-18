using MedAI.Contracts.Bookings;

namespace MedAI.Services;

public interface IBookingService
{
    Task<Result<BookingResponse>> CreateAsync(int doctorAvailableTimeId, CancellationToken cancellationToken = default);
    Task<Result<PaginatedList<PatientBookingResponse>>> GetMyBookingsAsync(RequestFilters filters,CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(int bookingId, CancellationToken cancellationToken = default);
    Task<Result<PaginatedList<DoctorBookingResponse>>> GetDoctorAppointmentsAsync(RequestFilters filters,CancellationToken cancellationToken = default);
}