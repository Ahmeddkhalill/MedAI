using MedAI.Contracts.Bookings;
using MedAI.Contracts.Xrays;

namespace MedAI.Contracts.Dashboard;

public record DoctorDashboardResponse(
    int UnrevisedCount,
    int TodayAppointmentsCount,
    int RevisedByMeTodayCount,
    IEnumerable<UnrevisedXrayResponse> UnrevisedList,
    IEnumerable<DoctorBookingResponse> TodayAppointmentsList
);