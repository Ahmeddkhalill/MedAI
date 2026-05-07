using MedAI.Contracts.Users;

namespace MedAI.Services;

public interface IUserService
{
    Task<Result<UserProfileResponse>> GetProfileAsync(string userId);
    Task<Result> ChangePasswordAsync(string userId, ChangePasswordRequest request);
    Task<Result> UpdateProfileAsync(string userId, UpdateProfileRequest request);
}
