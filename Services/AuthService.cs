namespace MedAI.Services;

public class AuthService(UserManager<ApplicationUser> userManager, IJwtProvider jwtProvider) : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly IJwtProvider _jwtProvider = jwtProvider;

    public async Task<Result<AuthResponse>> GetTokenAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(email);

        if (user is null)
            return Result.Failure<AuthResponse>(UserErrors.InvalidCredentials);

        var isValidPassword = await _userManager.CheckPasswordAsync(user, password);

        if (!isValidPassword)
            return Result.Failure<AuthResponse>(UserErrors.InvalidCredentials);

        return await GenerateAuthResult(user);
    }

    public async Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var emailExists = await _userManager.Users.AnyAsync(x => x.Email == request.Email, cancellationToken);

        if (emailExists)
            return Result.Failure<AuthResponse>(UserErrors.DuplicatedEmail);

        var user = request.Adapt<ApplicationUser>();

        var result = await _userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            var error = result.Errors.First();
            return Result.Failure<AuthResponse>(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
        }

        await _userManager.AddToRoleAsync(user, "Patient");

        return await GenerateAuthResult(user);
    }

    private async Task<Result<AuthResponse>> GenerateAuthResult(ApplicationUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);

        var (token, expiresIn) = _jwtProvider.GenerateToken(user, roles);

        var response = new AuthResponse(user.Id, user.Email, user.FirstName, user.LastName, token, expiresIn);

        return Result.Success(response);
    }
}
