using backend.DTOs;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous] // Login endpoint should be accessible without authentication
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(AuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Login endpoint - authenticates user and returns JWT token
    /// </summary>
    /// <param name="request">Login credentials (phone and password)</param>
    /// <returns>JWT token and user information</returns>
    /// <remarks>
    /// Sample request:
    /// 
    ///     POST /api/auth/login
    ///     {
    ///         "phone": "+1234567890",
    ///         "password": "password123"
    ///     }
    /// 
    /// Sample response:
    /// 
    ///     {
    ///         "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    ///         "userId": 1,
    ///         "businessId": 1,
    ///         "name": "John Doe",
    ///         "role": "Admin"
    ///     }
    /// </remarks>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        _logger.LogInformation("Login attempt received for phone: {Phone}", request?.Phone ?? "null");
        
        if (string.IsNullOrWhiteSpace(request?.Phone) || string.IsNullOrWhiteSpace(request?.Password))
        {
            _logger.LogWarning("Login failed: Missing phone or password");
            return BadRequest(new { message = "Phone and password are required" });
        }

        try
        {
            _logger.LogInformation("Attempting to authenticate user with phone: {Phone}", request.Phone);
            var response = await _authService.LoginAsync(request);

            if (response == null)
            {
                _logger.LogWarning("Login failed: Invalid credentials for phone: {Phone}", request.Phone);
                return Unauthorized(new { message = "Invalid phone or password" });
            }

            _logger.LogInformation("Login successful for user: {UserId}, BusinessId: {BusinessId}, Role: {Role}", 
                response.UserId, response.BusinessId, response.Role);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for phone: {Phone}", request?.Phone);
            return StatusCode(500, new { message = "An error occurred during login" });
        }
    }
}
