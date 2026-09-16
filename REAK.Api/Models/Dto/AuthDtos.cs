using System.ComponentModel.DataAnnotations;

namespace REAK.Api.Models.Dto;

public record LoginRequest([Required, EmailAddress] string Email, [Required] string Password);

public record RefreshRequest([Required] string RefreshToken);

public record LogoutRequest([Required] string RefreshToken);

public record ChangePasswordRequest([Required] string CurrentPassword, [Required, MinLength(8)] string NewPassword);

public record ForgotPasswordRequest([Required, EmailAddress] string Email);

public record ResetPasswordRequest([Required] string Token, [Required, MinLength(8)] string NewPassword);

public record TokenResponse(string AccessToken, DateTime AccessTokenExpiresAt, string RefreshToken, DateTime RefreshTokenExpiresAt);

public record CreateInvitationRequest([Required, EmailAddress] string Email, Guid? MemberEntityId, [Required] Guid RoleId);

public record AcceptInvitationRequest(string? Password);

public record SubmitMembershipApplicationRequest(
    [Required, MaxLength(255)] string CompanyName,
    [Required, MaxLength(255)] string ContactName,
    [Required, EmailAddress] string Email,
    [Required, MaxLength(50)] string Phone,
    [MaxLength(2000)] string? Message);

public record RejectMembershipApplicationRequest([Required, MaxLength(1000)] string Reason);
