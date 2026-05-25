namespace MedAI.Contracts.Dashboard;

public record DashboardResponse(
    DoctorStats Doctors,
    PatientStats Patients,
    BookingStats Bookings,
    XrayStats Xrays
);

public record DoctorStats(int Total);
public record PatientStats(int Total);
public record BookingStats(int Total, int Upcoming, int Completed, int Cancelled);
public record XrayStats(int Total, int Pending, int Edited, int Approved);