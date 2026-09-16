namespace REAK.Api.Services.Auth;

public record InvitationLookupResult(bool Found, string? Email, string? RoleName, string? MemberEntityName, bool IsNewAccount, bool IsExpiredOrUsed);
