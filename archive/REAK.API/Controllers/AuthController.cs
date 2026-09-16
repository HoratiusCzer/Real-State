using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using REAK.API.Models.DTOs;
using REAK.API.Services;

namespace REAK.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Register a new user with email, password, and role
    /// </summary>
    /// <param name="request">Registration details including email, password, and role</param>
    /// <returns>User information with JWT token (24-hour expiration)</returns>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Validation Error",
                    Detail = "Please check your input data",
                    Instance = HttpContext.Request.Path
                });
            }

            var response = await _authService.RegisterAsync(request);

            return CreatedAtAction(
                nameof(Register),
                new { id = response.Id },
                response
            );
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Registration failed: {Message}", ex.Message);
            return Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Registration Failed",
                Detail = ex.Message,
                Instance = HttpContext.Request.Path
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during registration");
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Internal Server Error",
                Detail = "An unexpected error occurred during registration",
                Instance = HttpContext.Request.Path
            });
        }
    }

    /// <summary>
    /// Login with email and password
    /// </summary>
    /// <param name="request">Login credentials (email and password)</param>
    /// <returns>User information with JWT token (24-hour expiration)</returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Validation Error",
                    Detail = "Please check your input data",
                    Instance = HttpContext.Request.Path
                });
            }

            var response = await _authService.LoginAsync(request);

            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Login failed for email: {Email}", request.Email);
            return Unauthorized(new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Authentication Failed",
                Detail = ex.Message,
                Instance = HttpContext.Request.Path
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during login");
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Internal Server Error",
                Detail = "An unexpected error occurred during login",
                Instance = HttpContext.Request.Path
            });
        }
    }

    /// <summary>
    /// Get current authenticated user information
    /// </summary>
    /// <returns>Current user details from JWT token</returns>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public ActionResult<object> GetCurrentUser()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
        var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
        var name = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;

        return Ok(new
        {
            Id = userId,
            Email = email,
            FullName = name,
            Role = role
        });
    }

    /// <summary>
    /// Health check endpoint for authentication service
    /// </summary>
    [HttpGet("health")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<object> Health()
    {
        return Ok(new
        {
            Service = "Authentication",
            Status = "Healthy",
            Timestamp = DateTime.UtcNow
        });
    }
}
