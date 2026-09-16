using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using REAK.API.Data;
using REAK.API.Models.DTOs;
using REAK.API.Models.Entities;

namespace REAK.API.Services;

public class AuthService : IAuthService
{
    private readonly ReakDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;

    public AuthService(ReakDbContext context, IConfiguration configuration, ILogger<AuthService> logger)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        // Check if user already exists
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower());

        if (existingUser != null)
        {
            throw new InvalidOperationException("User with this email already exists");
        }

        // Hash password with BCrypt
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        // Create new user
        var user = new User
        {
            Email = request.Email.ToLower(),
            PasswordHash = passwordHash,
            Role = request.Role,
            FullName = request.FullName,
            Phone = request.Phone,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        _logger.LogInformation("New user registered: {Email} with role {Role}", user.Email, user.Role);

        // Generate JWT token
        var token = GenerateJwtToken(user);

        return new AuthResponse
        {
            Id = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            Role = user.Role,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddHours(24)
        };
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        // Find user by email
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower());

        if (user == null)
        {
            throw new UnauthorizedAccessException("Invalid email or password");
        }

        // Check if user is active
        if (!user.IsActive)
        {
            throw new UnauthorizedAccessException("User account is disabled");
        }

        // Verify password with BCrypt
        var passwordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);

        if (!passwordValid)
        {
            throw new UnauthorizedAccessException("Invalid email or password");
        }

        // Update last login timestamp
        user.LastLoginAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        _logger.LogInformation("User logged in: {Email}", user.Email);

        // Generate JWT token
        var token = GenerateJwtToken(user);

        return new AuthResponse
        {
            Id = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            Role = user.Role,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddHours(24)
        };
    }

    private string GenerateJwtToken(User user)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey is not configured");
        var issuer = jwtSettings["Issuer"];
        var audience = jwtSettings["Audience"];

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim(ClaimTypes.Name, user.FullName ?? user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(24),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
