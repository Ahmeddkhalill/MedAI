using MedAI.Contracts.Bookings;

namespace MedAI.Contracts.Doctors;

public record DoctorAppointmentsByDateResponse(
    DateOnly Date,
    IEnumerable<DoctorBookingResponse> Appointments
);