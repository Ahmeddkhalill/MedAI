namespace MedAI.Services;

public interface IAuthService
{
    Task<Result<AuthResponse?>> LoginAsync(string email, string password, CancellationToken cancellationToken = default);
    Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
}
