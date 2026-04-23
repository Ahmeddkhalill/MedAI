using MedAI.Contracts.Doctors;

namespace MedAI.Services;

public class DoctorService(ApplicationDbContext context,UserManager<ApplicationUser> userManager) : IDoctorService
{
    private readonly ApplicationDbContext _context = context;
    private readonly UserManager<ApplicationUser> _userManager = userManager;

    public async Task<Result<PaginatedList<DoctorResponse>>> GetAllAsync(RequestFilters filters,CancellationToken cancellationToken = default)
    {
        var query = _context.Doctors
            .AsNoTracking()
            .Include(d => d.ApplicationUser)
            .Select(d => new DoctorResponse(
                d.Id,
                d.UserId,
                d.ApplicationUser.FirstName,
                d.ApplicationUser.LastName,
                d.ApplicationUser.Email!,
                d.Speciality,
                d.ImageUrl,
                d.Degree
            ));

        var response = await PaginatedList<DoctorResponse>.CreateAsync(query,filters.PageNumber,filters.PageSize);

        return Result.Success(response);
    }

    public async Task<Result<DoctorResponse>> GetByIdAsync(int id,CancellationToken cancellationToken = default)
    {
        var doctor = await _context.Doctors
            .Include(d => d.ApplicationUser)
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

        if (doctor is null)
            return Result.Failure<DoctorResponse>(DoctorErrors.NotFound);

        return Result.Success(MapToResponse(doctor, doctor.ApplicationUser));
    }

    public async Task<Result<DoctorResponse>> AddDoctorAsync(AddDoctorRequest request,CancellationToken cancellationToken = default)
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

        string? imageUrl = null;

        if (request.Image is not null)
            imageUrl = await SaveImageAsync(request.Image, cancellationToken);

        var doctor = new Doctor
        {
            UserId = user.Id,
            Degree = request.Degree,             
            Speciality = request.Speciality,       
            ImageUrl = imageUrl
        };

        _context.Doctors.Add(doctor);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(MapToResponse(doctor, user));
    }

    public async Task<Result> UpdateDoctorAsync(int id,UpdateDoctorRequest request,CancellationToken cancellationToken)
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

        if (request.Image is not null)
        {
            if (!string.IsNullOrEmpty(doctor.ImageUrl))
                DeleteOldImage(doctor.ImageUrl);

            doctor.ImageUrl = await SaveImageAsync(request.Image, cancellationToken);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> DeleteAsync(int id,CancellationToken cancellationToken = default)
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

    private static DoctorResponse MapToResponse(Doctor doctor, ApplicationUser user)
    {
        return new DoctorResponse(
            doctor.Id,
            user.Id,
            user.FirstName,
            user.LastName,
            user.Email!,
            doctor.Speciality,
            doctor.ImageUrl,
            doctor.Degree
        );
    }

    private async Task<string> SaveImageAsync(IFormFile image, CancellationToken cancellationToken)
    {
        var uploadsPath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "wwwroot",
            "images",
            "doctors");

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