using MedAI.Contracts.Bookings;
using MedAI.Contracts.Doctors;

namespace MedAI.Services;

public interface IBookingService
{
    Task<Result<BookingResponse>> CreateAsync(int doctorAvailableTimeId, CancellationToken cancellationToken = default);
    Task<Result<List<PatientBookingResponse>>> GetMyBookingsAsync(CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(int bookingId, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<DoctorAppointmentsByDateResponse>>> GetDoctorAppointmentsAsync(CancellationToken cancellationToken = default);
}