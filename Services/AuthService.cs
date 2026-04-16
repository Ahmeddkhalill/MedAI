using System.Security.Cryptography;

namespace MedAI.Services;

public class AuthService(
    UserManager<ApplicationUser> userManager,
    IJwtProvider jwtProvider,
    ApplicationDbContext context) : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly IJwtProvider _jwtProvider = jwtProvider;
    private readonly ApplicationDbContext _context = context;
    private readonly int _refreshTokenExpiryDays = 14;

    public async Task<Result<AuthResponse>> LoginAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(email);

        if (user is null)
            return Result.Failure<AuthResponse>(UserErrors.InvalidCredentials);

        var isValidPassword = await _userManager.CheckPasswordAsync(user, password);

        if (!isValidPassword)
            return Result.Failure<AuthResponse>(UserErrors.InvalidCredentials);

        return await GenerateAuthResult(user, cancellationToken);
    }

    public async Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var emailExists = await _userManager.Users
            .AnyAsync(x => x.Email == request.Email, cancellationToken);

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

        return await GenerateAuthResult(user, cancellationToken);
    }

    public async Task<Result<AuthResponse>> GetRefreshTokenAsync(string token, string refreshToken, CancellationToken cancellationToken = default)
    {
        var userId = _jwtProvider.ValidateToken(token);

        if (userId is null)
            return Result.Failure<AuthResponse>(UserErrors.InvalidJwtToken);

        var user = await _userManager.FindByIdAsync(userId);

        if (user is null)
            return Result.Failure<AuthResponse>(UserErrors.InvalidJwtToken);

        var userRefreshToken = await _context.RefreshTokens
            .SingleOrDefaultAsync(x => x.UserId == user.Id && x.Token == refreshToken && x.RevokedOn == null && x.ExpiresOn > DateTime.UtcNow, cancellationToken);

        if (userRefreshToken is null)
            return Result.Failure<AuthResponse>(UserErrors.InvalidRefreshToken);

        userRefreshToken.RevokedOn = DateTime.UtcNow;

        var roles = await _userManager.GetRolesAsync(user);
        var (newToken, expiresIn) = _jwtProvider.GenerateToken(user, roles);

        var newRefreshToken = new RefreshToken
        {
            Token = GenerateRefreshToken(),
            ExpiresOn = DateTime.UtcNow.AddDays(_refreshTokenExpiryDays),
            UserId = user.Id
        };

        await _context.RefreshTokens.AddAsync(newRefreshToken, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        var response = new AuthResponse(
            user.Id, user.Email, user.FirstName, user.LastName,
            newToken, expiresIn, newRefreshToken.Token, newRefreshToken.ExpiresOn);

        return Result.Success(response);
    }

    private async Task<Result<AuthResponse>> GenerateAuthResult(ApplicationUser user, CancellationToken cancellationToken = default)
    {
        var roles = await _userManager.GetRolesAsync(user);
        var (token, expiresIn) = _jwtProvider.GenerateToken(user, roles);

        var newRefreshToken = new RefreshToken
        {
            Token = GenerateRefreshToken(),
            ExpiresOn = DateTime.UtcNow.AddDays(_refreshTokenExpiryDays),
            UserId = user.Id  
        };

        await _context.RefreshTokens.AddAsync(newRefreshToken, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        var response = new AuthResponse(
            user.Id, user.Email, user.FirstName, user.LastName,
            token, expiresIn, newRefreshToken.Token, newRefreshToken.ExpiresOn);

        return Result.Success(response);
    }

    private static string GenerateRefreshToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
    }
}