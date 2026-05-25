using MedAI.Contracts.Bookings;
using MedAI.Contracts.Xrays;

namespace MedAI.Contracts.Dashboard;

public record PatientDashboardResponse(
    int TotalAnalysis,
    int RevisedAnalysis,
    int UnrevisedAnalysis,
    IEnumerable<PatientXrayHistoryResponse> RecentAnalysis,
    PatientBookingResponse? NextAppointment
);