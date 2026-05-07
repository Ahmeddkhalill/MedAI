using MedAI.Contracts.Admin;

namespace MedAI.Services;

public class AdminService(ApplicationDbContext context) : IAdminService
{
    private readonly ApplicationDbContext _context = context;

    public async Task<Result<DashboardResponse>> GetDashboardAsync(
    CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var totalDoctors = await _context.Doctors.CountAsync(cancellationToken);

        var totalPatients = await (
                                from user in _context.Users
                                join userRole in _context.UserRoles
                                    on user.Id equals userRole.UserId
                                join role in _context.Roles
                                    on userRole.RoleId equals role.Id
                                where role.Name == "Patient"
                                select user
                            ).CountAsync(cancellationToken);

        var totalBookings = await _context.Bookings
            .CountAsync(cancellationToken);

        var totalXrays = await _context.Xrays
            .CountAsync(cancellationToken);

        var revisedXrays = await _context.Xrays
            .CountAsync(x => x.IsRevised, cancellationToken);

        var pendingXrays = await _context.Xrays
            .CountAsync(x => !x.IsRevised, cancellationToken);

        var upcomingBookings = await _context.Bookings
            .CountAsync(x =>
                x.DoctorAvailableTime.Date >= today,
                cancellationToken);

        var completedBookings = await _context.Bookings
            .CountAsync(x =>
                x.DoctorAvailableTime.Date < today,
                cancellationToken);

        var response = new DashboardResponse(
            totalDoctors,
            totalPatients,
            totalBookings,
            totalXrays,
            revisedXrays,
            pendingXrays,
            upcomingBookings,
            completedBookings
        );

        return Result.Success(response);
    }
}
