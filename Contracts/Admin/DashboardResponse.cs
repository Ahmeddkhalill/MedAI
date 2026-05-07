namespace MedAI.Contracts.Admin;

public record DashboardResponse(
    int TotalDoctors,
    int TotalPatients,
    int TotalBookings,
    int TotalXrays,
    int RevisedXrays,
    int PendingXrays,
    int UpcomingBookings,
    int CompletedBookings
);