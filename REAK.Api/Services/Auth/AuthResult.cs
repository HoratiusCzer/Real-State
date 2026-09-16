namespace REAK.Api.Services.Auth;

public record AuthTokens(string AccessToken, DateTime AccessTokenExpiresAt, string RefreshToken, DateTime RefreshTokenExpiresAt);

public record AuthResult(bool Success, string? Error, AuthTokens? Tokens);
