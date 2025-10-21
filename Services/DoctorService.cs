using MedAI.Contracts.Doctors;

namespace MedAI.Services;

public class DoctorService(ApplicationDbContext context, UserManager<ApplicationUser> userManager) : IDoctorService
{
    private readonly ApplicationDbContext _context = context;
    private readonly UserManager<ApplicationUser> _userManager = userManager;

    public async Task<Result<DoctorResponse>> AddDoctorAsync(AddDoctorRequest request, CancellationToken cancellationToken = default)
    {
        var emailExists = _userManager.Users.Any(x => x.Email == request.Email);

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
            return Result.Failure<DoctorResponse>(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
        }

        await _userManager.AddToRoleAsync(user, "Doctor");

        var doctor = new Doctor
        {
            UserId = user.Id,
            Degree = request.Degree
        };

        _context.Doctors.Add(doctor);
        await _context.SaveChangesAsync();

        var response = new DoctorResponse(doctor.Id, user.Id, user.FirstName, user.LastName, user.Email, doctor.Degree);

        return Result.Success(response);
    }

    public async Task<Result<DoctorResponse>> UpdateDoctorAsync(int id, UpdateDoctorRequest request, CancellationToken cancellationToken)
    {
        var doctor = await _context.Doctors
            .Include(d => d.ApplicationUser)
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

        if (doctor is null)
            return Result.Failure<DoctorResponse>(DoctorErrors.NotFound);

        var emailExists = await _userManager.Users
            .AnyAsync(u => u.Email == request.Email && u.Id != doctor.UserId, cancellationToken);

        if (emailExists)
            return Result.Failure<DoctorResponse>(DoctorErrors.DuplicatedEmail);

        doctor.ApplicationUser.FirstName = request.FirstName;
        doctor.ApplicationUser.LastName = request.LastName;
        doctor.ApplicationUser.Email = request.Email;
        doctor.Degree = request.Degree;

        await _context.SaveChangesAsync(cancellationToken);

        var response = new DoctorResponse(
            doctor.Id,
            doctor.UserId,
            doctor.ApplicationUser.FirstName,
            doctor.ApplicationUser.LastName,
            doctor.ApplicationUser.Email,
            doctor.Degree
        );

        return Result.Success(response);
    }

    public async Task<Result<IEnumerable<DoctorResponse>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var doctors = await _context.Doctors
            .Include(d => d.ApplicationUser)
            .ToListAsync(cancellationToken);

        var responses = doctors.Select(d => new DoctorResponse(
            d.Id,
            d.UserId,
            d.ApplicationUser.FirstName,
            d.ApplicationUser.LastName,
            d.ApplicationUser.Email!,
            d.Degree
        ));

        return Result.Success(responses);
    }

    public async Task<Result<DoctorResponse>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var doctor = await _context.Doctors
            .Include(d => d.ApplicationUser)
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

        if (doctor is null)
            return Result.Failure<DoctorResponse>(DoctorErrors.NotFound);

        var response = new DoctorResponse(
            doctor.Id,
            doctor.UserId,
            doctor.ApplicationUser.FirstName,
            doctor.ApplicationUser.LastName,
            doctor.ApplicationUser.Email!,
            doctor.Degree
        );

        return Result.Success(response);
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var doctor = await _context.Doctors
            .Include(d => d.ApplicationUser)
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

        if (doctor is null)
            return Result.Failure(DoctorErrors.NotFound);

        _context.Doctors.Remove(doctor);
        await _userManager.DeleteAsync(doctor.ApplicationUser);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}