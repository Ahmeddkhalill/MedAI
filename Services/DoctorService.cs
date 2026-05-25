using MedAI.Contracts.Bookings;
using MedAI.Contracts.Dashboard;
using MedAI.Contracts.Doctors;
using MedAI.Contracts.Xrays;

namespace MedAI.Services;

public class DoctorService(
    ApplicationDbContext context,
    UserManager<ApplicationUser> userManager,
    IHttpContextAccessor httpContextAccessor) : IDoctorService
{
    private readonly ApplicationDbContext _context = context;
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    public async Task<Result<PaginatedList<DoctorResponse>>> GetAllAsync(RequestFilters filters, CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var query = _context.Doctors
            .AsNoTracking()
            .Include(d => d.ApplicationUser)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filters.SearchValue))
        {
            var search = $"%{filters.SearchValue}%";
            query = query.Where(d =>
                EF.Functions.Like(d.ApplicationUser.FirstName, search) ||
                EF.Functions.Like(d.ApplicationUser.LastName, search) ||
                EF.Functions.Like(d.Speciality, search)
            );
        }

        var projected = query.Select(d => new DoctorResponse(
            d.Id,
            d.UserId,
            d.ApplicationUser.FirstName,
            d.ApplicationUser.LastName,
            d.ApplicationUser.Email!,
            d.Speciality,
            d.ImageUrl,
            d.Degree,
            d.Description,
            d.IsAccountCompleted,
            d.AvailableTimes
                .SelectMany(at => at.Bookings)
                .Count(b => !b.IsCancelled && b.DoctorAvailableTime.Date < today),
            _context.Xrays
                .Count(x => x.DoctorId == d.Id && (x.IsEdited || x.IsApproved))
        ));

        var response = await PaginatedList<DoctorResponse>.CreateAsync(projected, filters.PageNumber, filters.PageSize);
        return Result.Success(response);
    }

    public async Task<Result<DoctorDashboardResponse>> GetDoctorDashboardAsync(CancellationToken cancellationToken = default)
    {
        var userId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId is null)
            return Result.Failure<DoctorDashboardResponse>(DoctorErrors.Unauthorized);

        var doctor = await _context.Doctors
            .FirstOrDefaultAsync(d => d.UserId == userId, cancellationToken);

        if (doctor is null)
            return Result.Failure<DoctorDashboardResponse>(DoctorErrors.NotFound);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var todayStart = DateTime.UtcNow.Date;
        var todayEnd = todayStart.AddDays(1);

        var unrevisedCount = await _context.Xrays
            .CountAsync(x => !x.IsEdited && !x.IsApproved, cancellationToken);

        var todayAppointmentsCount = await _context.Bookings
            .CountAsync(b => b.DoctorAvailableTime.DoctorId == doctor.Id
                          && b.DoctorAvailableTime.Date == today
                          && !b.IsCancelled, cancellationToken);

        var revisedByMeTodayCount = await _context.Xrays
            .CountAsync(x => x.DoctorId == doctor.Id
                          && x.ConfirmedAt >= todayStart
                          && x.ConfirmedAt < todayEnd, cancellationToken);

        var unrevisedList = await _context.Xrays
            .AsNoTracking()
            .Include(x => x.Patient)
            .Where(x => !x.IsEdited && !x.IsApproved)
            .OrderBy(x => x.CreatedAt)
            .Take(3)
            .Select(x => new UnrevisedXrayResponse(
                x.Id,
                x.ImageUrl,
                x.AI_Diagnosis,
                x.AI_Confidence,
                x.PatientId,
                x.Patient.FirstName + " " + x.Patient.LastName,
                x.CreatedAt
            ))
            .ToListAsync(cancellationToken);

        var todayAppointmentsList = await _context.Bookings
            .AsNoTracking()
            .Include(b => b.Patient)
            .Include(b => b.DoctorAvailableTime)
            .Where(b => b.DoctorAvailableTime.DoctorId == doctor.Id
                     && b.DoctorAvailableTime.Date == today
                     && !b.IsCancelled)
            .OrderBy(b => b.DoctorAvailableTime.StartTime)
            .Take(5)
            .Select(b => new DoctorBookingResponse(
                b.Id,
                b.CreatedAt,
                "Upcoming",
                b.PatientId,
                b.Patient.FirstName,
                b.Patient.LastName,
                b.Patient.Email!,
                b.DoctorAvailableTime.Id,
                b.DoctorAvailableTime.Date,
                b.DoctorAvailableTime.StartTime,
                b.DoctorAvailableTime.EndTime,
                b.DoctorAvailableTime.ConsultationFee
            ))
            .ToListAsync(cancellationToken);

        var response = new DoctorDashboardResponse(
            unrevisedCount,
            todayAppointmentsCount,
            revisedByMeTodayCount,
            unrevisedList,
            todayAppointmentsList
        );

        return Result.Success(response);
    }
    public async Task<Result<DoctorResponse>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var doctor = await _context.Doctors
            .Include(d => d.ApplicationUser)
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

        if (doctor is null)
            return Result.Failure<DoctorResponse>(DoctorErrors.NotFound);

        var completedAppointments = await _context.Bookings
            .CountAsync(b => b.DoctorAvailableTime.DoctorId == doctor.Id
                          && !b.IsCancelled
                          && b.DoctorAvailableTime.Date < today, cancellationToken);

        var handledXrays = await _context.Xrays
            .CountAsync(x => x.DoctorId == doctor.Id
                          && (x.IsEdited || x.IsApproved), cancellationToken);

        return Result.Success(MapToResponse(doctor, doctor.ApplicationUser, completedAppointments, handledXrays));
    }

    public async Task<Result<DoctorResponse>> GetMyProfileAsync(CancellationToken cancellationToken = default)
    {
        var userId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId is null)
            return Result.Failure<DoctorResponse>(DoctorErrors.Unauthorized);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var doctor = await _context.Doctors
            .Include(d => d.ApplicationUser)
            .FirstOrDefaultAsync(d => d.UserId == userId, cancellationToken);

        if (doctor is null)
            return Result.Failure<DoctorResponse>(DoctorErrors.NotFound);

        var completedAppointments = await _context.Bookings
            .CountAsync(b => b.DoctorAvailableTime.DoctorId == doctor.Id
                          && !b.IsCancelled
                          && b.DoctorAvailableTime.Date < today, cancellationToken);

        var handledXrays = await _context.Xrays
            .CountAsync(x => x.DoctorId == doctor.Id
                          && (x.IsEdited || x.IsApproved), cancellationToken);

        return Result.Success(MapToResponse(doctor, doctor.ApplicationUser, completedAppointments, handledXrays));
    }

    public async Task<Result<DoctorResponse>> AddDoctorAsync(AddDoctorRequest request, CancellationToken cancellationToken = default)
    {
        var emailExists = await _userManager.Users
            .AnyAsync(x => x.Email == request.Email, cancellationToken);

        if (emailExists)
            return Result.Failure<DoctorResponse>(DoctorErrors.DuplicatedEmail);

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName
        };

        var result = await _userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            var error = result.Errors.First();
            return Result.Failure<DoctorResponse>(
                new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
        }

        await _userManager.AddToRoleAsync(user, "Doctor");

        var doctor = new Doctor
        {
            UserId = user.Id,
            Degree = request.Degree,
            Speciality = request.Speciality,
            IsAccountCompleted = false
        };

        _context.Doctors.Add(doctor);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(MapToResponse(doctor, user));
    }

    public async Task<Result> UpdateDoctorAsync(int id, UpdateDoctorRequest request, CancellationToken cancellationToken)
    {
        var doctor = await _context.Doctors
            .Include(d => d.ApplicationUser)
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

        if (doctor is null)
            return Result.Failure(DoctorErrors.NotFound);

        var emailExists = await _userManager.Users
            .AnyAsync(u => u.Email == request.Email && u.Id != doctor.UserId, cancellationToken);

        if (emailExists)
            return Result.Failure(DoctorErrors.DuplicatedEmail);

        doctor.ApplicationUser.Email = request.Email;
        doctor.ApplicationUser.UserName = request.Email;
        doctor.Degree = request.Degree;
        doctor.Speciality = request.Speciality;

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> CompleteProfileAsync(CompleteProfileRequest request, CancellationToken cancellationToken = default)
    {
        var userId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId is null)
            return Result.Failure(DoctorErrors.Unauthorized);

        var doctor = await _context.Doctors
            .Include(d => d.ApplicationUser)
            .FirstOrDefaultAsync(d => d.UserId == userId, cancellationToken);

        if (doctor is null)
            return Result.Failure(DoctorErrors.NotFound);

        doctor.ApplicationUser.FirstName = request.FirstName;
        doctor.ApplicationUser.LastName = request.LastName;
        doctor.Description = request.Description;

        if (request.Image is not null)
        {
            if (!string.IsNullOrEmpty(doctor.ImageUrl))
                DeleteOldImage(doctor.ImageUrl);

            doctor.ImageUrl = await SaveImageAsync(request.Image, cancellationToken);
        }

        doctor.IsAccountCompleted = true;

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var doctor = await _context.Doctors
            .Include(d => d.ApplicationUser)
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

        if (doctor is null)
            return Result.Failure(DoctorErrors.NotFound);

        if (!string.IsNullOrEmpty(doctor.ImageUrl))
            DeleteOldImage(doctor.ImageUrl);

        _context.Doctors.Remove(doctor);
        await _userManager.DeleteAsync(doctor.ApplicationUser);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private static DoctorResponse MapToResponse(Doctor doctor, ApplicationUser user, int completedAppointments = 0, int handledXrays = 0)
    {
        return new DoctorResponse(
            doctor.Id,
            user.Id,
            user.FirstName,
            user.LastName,
            user.Email!,
            doctor.Speciality,
            doctor.ImageUrl,
            doctor.Degree,
            doctor.Description,
            doctor.IsAccountCompleted,
            completedAppointments,
            handledXrays
        );
    }

    private async Task<string> SaveImageAsync(IFormFile image, CancellationToken cancellationToken)
    {
        var uploadsPath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "wwwroot", "images", "doctors");

        if (!Directory.Exists(uploadsPath))
            Directory.CreateDirectory(uploadsPath);

        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(image.FileName)}";
        var fullPath = Path.Combine(uploadsPath, fileName);

        using var stream = new FileStream(fullPath, FileMode.Create);
        await image.CopyToAsync(stream, cancellationToken);

        return $"/images/doctors/{fileName}";
    }

    private void DeleteOldImage(string imageUrl)
    {
        var oldPath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "wwwroot",
            imageUrl.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));

        if (File.Exists(oldPath))
            File.Delete(oldPath);
    }
}