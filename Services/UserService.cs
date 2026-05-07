using MedAI.Contracts.Users;

namespace MedAI.Services;

public class UserService(UserManager<ApplicationUser> userManager) : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;

    public async Task<Result<UserProfileResponse>> GetProfileAsync(string userId)
    {
        var user = await _userManager.Users
            .Include(u => u.Doctor)
            .Where(x => x.Id == userId)
            .Select(u => new UserProfileResponse(
                u.Email!,
                u.UserName!,
                u.FirstName,
                u.LastName,
                u.Doctor != null ? u.Doctor.IsAccountCompleted : null
            ))
            .SingleOrDefaultAsync();

        if (user is null)
            return Result.Failure<UserProfileResponse>(UserErrors.UserNotFound);

        return Result.Success(user);
    }

    public async Task<Result> ChangePasswordAsync(string userId, ChangePasswordRequest request)
    {
        var user = await _userManager.FindByIdAsync(userId);
        
        var result = await _userManager.ChangePasswordAsync(user!, request.CurrentPassword, request.NewPassword);
        
        if (result.Succeeded)
            return Result.Success();

        var error = result.Errors.First();
        
        return Result.Failure(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
    }

    public async Task<Result> UpdateProfileAsync(string userId, UpdateProfileRequest request)
    {
        var user = await _userManager.FindByIdAsync(userId);

        user = request.Adapt(user);

        await _userManager.UpdateAsync(user!);

        return Result.Success();
    }
}
