using MedAI.Contracts.Bookings;
using MedAI.Contracts.Doctors;

namespace MedAI.Services;

public interface IBookingService
{
    Task<Result<BookingResponse>> CreateAsync(int scheduleId, CancellationToken cancellationToken = default);
    Task<Result<ListResponse<PatientBookingResponse>>> GetMyBookingsAsync(RequestFilters filters,CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(int bookingId, CancellationToken cancellationToken = default);
    Task<Result<PaginatedList<DoctorAppointmentsByDateResponse>>> GetDoctorAppointmentsAsync(RequestFilters filters, CancellationToken cancellationToken = default);
}