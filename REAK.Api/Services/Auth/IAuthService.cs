namespace REAK.Api.Services.Auth;

public interface IAuthService
{
    Task<AuthResult> LoginAsync(string email, string password, string? ip, CancellationToken ct = default);
    Task<AuthResult> RefreshAsync(string rawRefreshToken, string? ip, CancellationToken ct = default);
    Task LogoutAsync(string rawRefreshToken, CancellationToken ct = default);
    Task<bool> ChangePasswordAsync(Guid profileId, string currentPassword, string newPassword, CancellationToken ct = default);
    Task ForgotPasswordAsync(string email, CancellationToken ct = default);
    Task<bool> ResetPasswordAsync(string rawToken, string newPassword, CancellationToken ct = default);
}
